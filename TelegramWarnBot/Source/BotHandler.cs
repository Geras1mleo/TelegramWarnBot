namespace TelegramWarnBot;

public static class BotHandler
{
    public static Task HandlePollingErrorAsync(ITelegramBotClient client, Exception exception, CancellationToken cancellationToken) => Task.CompletedTask;

    public static async Task HandleUpdateAsync(ITelegramBotClient client, Update update, CancellationToken cancellationToken)
    {
        try
        {
            await HandleBotJoinedOrLeftAsync(client, update, cancellationToken);

            // Update must be a valid message with a From-user
            if (!IsValidSender(update))
                return;

            IOHandler.RegisterUser(update.Message.From.Id,
                                   update.Message.From.Username,
                                   update.Message.From.GetFullName());

            IOHandler.RegisterChat(update.Message.Chat.Id, update.Message.Chat.Title);

            if (update.Message.Text is null)
                return;

            await ResolveTriggersAsync(client, update.Message.Text, update.Message.Chat.Id, update.Message.MessageId, cancellationToken);
            await ResolveIllegalTriggersAsync(client, update, cancellationToken);

            // Check if message is a command
            if (!IsValidCommand(update.Message.Text))
                return;

            var method = Tools.ResolveMethod(typeof(WarnController), update.Message.Text.Split(' ')[0][1..]);

            if (method is null)
                return;

            var response = await (Task<BotResponse>)(method.Invoke(Activator.CreateInstance(typeof(WarnController), new UserService()),
                           new object[] { client, update, cancellationToken }) ?? "Executed!");

            // No response provided
            if (response is null)
                return;

            await client.SendTextMessageAsync(update.Message.Chat.Id,
                                             response.Data,
                                             cancellationToken: cancellationToken,
                                             parseMode: ParseMode.Markdown);
        }
        catch (Exception e)
        {
            // Update that raised exception will be saved in Logs.json
            // Bot will skip this message, bot will not handle it ever again
            await IOHandler.LogErrorAsync(new() { Exception = e, Update = update, Time = DateTime.Now });
            Tools.WriteColor($"[HandlePollingErrorAsync]\n[Message]: {e.Message}\n[StackTrace]: {e.StackTrace}", ConsoleColor.Red, true);
        }
    }

    private static Task HandleBotJoinedOrLeftAsync(ITelegramBotClient client, Update update, CancellationToken cancellationToken)
    {
        // If bot has been added to new chat
        if (update.Message.Type == MessageType.ChatMembersAdded
         && update.Message.NewChatMembers.Any(m => m.Id == Bot.User.Id))
        {
            return client.SendTextMessageAsync(update.Message.Chat.Id,
                   IOHandler.GetConfiguration().Captions.OnBotJoinedChatMessage,
                   cancellationToken: cancellationToken, parseMode: ParseMode.Markdown);
        }
        // If bot left chat / kicked from chat => clear chats
        else if (update.Message.Type == MessageType.ChatMemberLeft
              && update.Message.LeftChatMember.Id == Bot.User.Id)
        {
            IOHandler.GetWarnings().RemoveAll(c => c.ChatId == update.Message.Chat.Id);
        }

        return Task.CompletedTask;
    }

    private static async Task ResolveTriggersAsync(ITelegramBotClient client, string message, long chatId, int messageId, CancellationToken cancellationToken)
    {
        foreach (var trigger in IOHandler.GetTriggers())
        {
            if (trigger.Chat is not null && trigger.Chat != chatId)
                continue;

            if (MatchMessage(trigger.Messages, trigger.MatchWholeMessage, trigger.MatchCase, message))
            {
                await client.SendTextMessageAsync(chatId,
                                                trigger.Response,
                                                replyToMessageId: messageId,
                                                cancellationToken: cancellationToken,
                                                parseMode: ParseMode.Markdown);
                return;
            }
        }
    }

    private static async Task ResolveIllegalTriggersAsync(ITelegramBotClient client, Update update, CancellationToken cancellationToken)
    {
        // Illegal triggers are disabled for admins
        if (await new UserService().IsAdmin(client, update.Message.Chat.Id, update.Message.From.Id, cancellationToken))
            return;

        foreach (var trigger in IOHandler.GetIllegalTriggers())
        {
            if (trigger.Chat is not null && trigger.Chat != update.Message.Chat.Id)
                continue;

            if (MatchMessage(trigger.IllegalWords, false, false, update.Message.Text))
            {
                foreach (var adminId in trigger.NotifiedAdmins)
                {
                    await client.SendTextMessageAsync(adminId,
                                                    $"*Illegal message detected!*\nChat: *{update.Message.Chat.Title}*" +
                                                    $"\nFrom: *{update.Message.From?.GetFullName()}*" +
                                                    $"\nSent: {update.Message.Date}" +
                                                    $"\nContent:",
                                                    cancellationToken: cancellationToken,
                                                    parseMode: ParseMode.Markdown);

                    await client.ForwardMessageAsync(adminId, update.Message.Chat.Id, update.Message.MessageId, cancellationToken: cancellationToken);
                }
                if (trigger.WarnMember)
                {
                    var service = new UserService();

                    var chat = service.ResolveChat(update, IOHandler.GetWarnings());
                    var user = service.ResolveWarnedUser(update.Message.From.Id, chat);

                    var banned = await service.Warn(user, chat.ChatId,
                                              trigger.DeleteMessage ? update.Message.MessageId : null,
                                              client, cancellationToken);


                    await client.SendTextMessageAsync(update.Message.Chat.Id,
                                                      Tools.ResolveResponseVariables(
                                                          banned ? IOHandler.GetConfiguration().Captions.IllegalTriggerBanned
                                                                 : IOHandler.GetConfiguration().Captions.IllegalTriggerWarned, user, update.Message.From.GetFullName()),
                                                      cancellationToken: cancellationToken,
                                                      parseMode: ParseMode.Markdown);
                }
                else if (trigger.DeleteMessage)
                {
                    await client.DeleteMessageAsync(update.Message.Chat.Id, update.Message.MessageId, cancellationToken);
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
