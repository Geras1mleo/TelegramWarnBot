namespace TelegramWarnBot;

public static class BotHandler
{
    public static Task HandleUpdateAsync(ITelegramBotClient client, Update update, CancellationToken cancellationToken)
    {
        var tasks = new List<Task>();
        try
        {
            // Update must be a valid message with a From-user
            if (!IsValidSender(update))
                return Task.CompletedTask;

            var (updateHandled, handlingTask) = HandleJoinedLeftUpdateAsync(client, update, cancellationToken);

            if (updateHandled) return handlingTask;

            if (Bot.IsChatRegistered(update.Message.Chat.Id))
            {
                IOHandler.CacheUser(update.Message.From);
            }

            if (update.Message.Text is null)
                return Task.CompletedTask;

            if (Bot.IsChatRegistered(update.Message.Chat.Id))
            {
                tasks.Add(ResolveTriggersAsync(client, update.Message, cancellationToken));
                tasks.Add(ResolveIllegalTriggersAsync(client, update.Message, cancellationToken));
            }

            // Check if message is a command
            if (IsValidCommand(update.Message.Text))
            {
                var method = Tools.ResolveMethod(typeof(WarnController), update.Message.Text.Split(' ')[0][1..]);

                // Method not found
                if (method is null)
                    return Task.CompletedTask;

                if (!Bot.IsChatRegistered(update.Message.Chat.Id))
                {
                    tasks.Add(client.SendTextMessageAsync(update.Message.Chat.Id,
                                                          IOHandler.Configuration.Captions.ChatNotRegistered,
                                                          cancellationToken: cancellationToken,
                                                          parseMode: ParseMode.Markdown));
                    return Task.WhenAll(tasks);
                }

                var response = ((Task<BotResponse>)
                                (method.Invoke(Activator.CreateInstance(
                                                typeof(WarnController),
                                                UserService.Shared),
                                                    new object[]
                                                    {
                                                        client,
                                                        update,
                                                        cancellationToken
                                                    }) ?? "Executed!"))
                                                        .GetAwaiter()
                                                        .GetResult();

                // No response provided
                if (response is null)
                    Task.WhenAll(tasks);

                tasks.Add(client.SendTextMessageAsync(update.Message.Chat.Id,
                                                      response.Data,
                                                      cancellationToken: cancellationToken,
                                                      parseMode: ParseMode.Markdown));
            }
        }
        catch (Exception e)
        {
            // Update that raised exception will be saved in Logs.json
            // Bot will skip this message, bot will not handle it ever again
            IOHandler.Logs.Add(new()
            {
                Update = update,
                Time = DateTime.Now,
                Exception = new()
                {
                    Message = e.Message,
                    StackTrace = e.StackTrace
                },
            });
            Tools.WriteColor($"[HandlePollingErrorAsync]\n[Message]: {e.Message}\n[StackTrace]: {e.StackTrace}", ConsoleColor.Red, true);
        }

        return Task.WhenAll(tasks);
    }

    private static (bool updateHandled, Task handlingTask) HandleJoinedLeftUpdateAsync(ITelegramBotClient client, Update update, CancellationToken cancellationToken)
    {
        if (update.Message.Type == MessageType.ChatMembersAdded)
        {
            // If bot self has been added to new chat => greeting message
            if (update.Message.NewChatMembers.Any(m => m.Id == Bot.User.Id))
            {
                IOHandler.CacheChat(update.Message.Chat, UserService.Shared.GetAdmins(client, update.Message.Chat.Id, cancellationToken));

                return (true, client.SendTextMessageAsync(update.Message.Chat.Id,
                          IOHandler.Configuration.Captions.OnBotJoinedChatMessage,
                          cancellationToken: cancellationToken, parseMode: ParseMode.Markdown));
            }

            return (true, HandleJoinedAsync(client, update, cancellationToken));
        }
        else if (update.Message.Type == MessageType.ChatMemberLeft)
        {
            // If bot left chat / kicked from chat => clear data
            if (update.Message.LeftChatMember.Id == Bot.User.Id)
            {
                IOHandler.Warnings.RemoveAll(w => w.ChatId == update.Message.Chat.Id);
                IOHandler.Chats.RemoveAll(c => c.Id == update.Message.Chat.Id);

                return (true, Task.CompletedTask);
            }

            return (true, HandleLeftAsync(client, update, cancellationToken));
        }

        return (false, Task.CompletedTask);
    }

    private static Task HandleJoinedAsync(ITelegramBotClient client, Update update, CancellationToken cancellationToken)
    {
        if (!Bot.IsChatRegistered(update.Message.Chat.Id))
            return Task.CompletedTask;

        if (IOHandler.Configuration.DeleteJoinedLeftMessage)
        {
            if (UserService.Shared.IsAdmin(update.Message.Chat.Id, Bot.User.Id))
                return client.DeleteMessageAsync(update.Message.Chat.Id, update.Message.MessageId, cancellationToken);
        }

        foreach (var member in update.Message.NewChatMembers)
        {
            if (!member.IsBot)
                IOHandler.CacheUser(member);
        }

        return Task.CompletedTask;
    }

    private static Task HandleLeftAsync(ITelegramBotClient client, Update update, CancellationToken cancellationToken)
    {
        if (!Bot.IsChatRegistered(update.Message.Chat.Id))
            return Task.CompletedTask;

        else if (IOHandler.Configuration.DeleteJoinedLeftMessage)
        {
            if (UserService.Shared.IsAdmin(update.Message.Chat.Id, Bot.User.Id))
                return client.DeleteMessageAsync(update.Message.Chat.Id, update.Message.MessageId, cancellationToken);
        }

        return Task.CompletedTask;
    }

    private static Task ResolveTriggersAsync(ITelegramBotClient client, Message message, CancellationToken cancellationToken)
    {
        foreach (var trigger in IOHandler.Triggers)
        {
            if (trigger.Chat is not null && trigger.Chat != message.Chat.Id)
                continue;

            if (MatchMessage(trigger.Messages, trigger.MatchWholeMessage, trigger.MatchCase, message.Text))
            {
                return client.SendTextMessageAsync(message.Chat.Id,
                                                   trigger.Response,
                                                   replyToMessageId: message.MessageId,
                                                   cancellationToken: cancellationToken,
                                                   parseMode: ParseMode.Markdown);
            }
        }
        return Task.CompletedTask;
    }

    private static async Task ResolveIllegalTriggersAsync(ITelegramBotClient client, Message message, CancellationToken cancellationToken)
    {
        var isAdmin = UserService.Shared.IsAdmin(message.Chat.Id, message.From.Id);

        foreach (var trigger in IOHandler.IllegalTriggers)
        {
            // Illegal triggers => ignore admins?
            if (trigger.IgnoreAdmins && isAdmin)
                continue;

            // Applicapble in specific chat
            if (trigger.Chat is not null && trigger.Chat != message.Chat.Id)
                continue;

            if (MatchMessage(trigger.IllegalWords, false, false, message.Text))
            {
                foreach (var adminId in trigger.NotifiedAdmins)
                {
                    await client.SendTextMessageAsync(adminId,
                                                      $"*Illegal message detected!*\nChat: *{message.Chat.Title}*" +
                                                      $"\nFrom: *{message.From?.GetFullName()}*" +
                                                      $"\nSent: {message.Date}" +
                                                      $"\nContent:",
                                                      cancellationToken: cancellationToken,
                                                      parseMode: ParseMode.Markdown);

                    await client.ForwardMessageAsync(adminId, message.Chat.Id,
                                                     message.MessageId,
                                                     cancellationToken: cancellationToken);
                }

                // Notify but don't warn admins and dont delete message if not allowed in config
                if (isAdmin && !IOHandler.Configuration.AllowAdminWarnings)
                    return;

                if (trigger.WarnMember)
                {

                    var service = UserService.Shared;

                    var chat = service.ResolveChatWarning(message.Chat.Id, IOHandler.Warnings);
                    var user = service.ResolveWarnedUser(message.From.Id, chat);

                    var banned = await service.Warn(user, chat.ChatId, null, !isAdmin, client, cancellationToken);


                    await client.SendTextMessageAsync(message.Chat.Id,
                                                      Tools.ResolveResponseVariables(
                                                          banned ? IOHandler.Configuration.Captions.IllegalTriggerBanned
                                                                 : IOHandler.Configuration.Captions.IllegalTriggerWarned,
                                                          user,
                                                          message.From.GetFullName()),
                                                      cancellationToken: cancellationToken,
                                                      parseMode: ParseMode.Markdown);
                }

                if (trigger.DeleteMessage)
                {
                    await client.DeleteMessageAsync(message.Chat.Id, message.MessageId, cancellationToken);
                }

                // Match only 1 trigger
                return;
            }
        }
    }

    private static bool MatchMessage(string[] matchFromMessages, bool matchWholeMessage, bool matchCase, string message)
    {
        if (matchWholeMessage)
            return matchFromMessages.Any(m => matchCase ? m == message : m.ToLower() == message.ToLower());

        return matchFromMessages.Any(m => message.Contains(m, matchCase ? StringComparison.CurrentCulture : StringComparison.CurrentCultureIgnoreCase));
    }

    private static bool IsValidSender(Update update)
    {
        return update.Message is not null
            && update.Message.From is not null
            && (update.Message.Chat.Type == ChatType.Group || update.Message.Chat.Type == ChatType.Supergroup);
    }

    private static bool IsValidCommand(string message)
    {
        var parts = message.Split(' ');
        return parts.Length > 0
            && parts[0].StartsWith('/');
    }
}
