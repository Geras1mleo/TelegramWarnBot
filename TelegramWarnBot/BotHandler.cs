namespace TelegramWarnBot;

public static class BotHandler
{
    public static Task HandlePollingErrorAsync(ITelegramBotClient client, Exception exception, CancellationToken cancellationToken) => Task.CompletedTask;

    public static Task HandleUpdateAsync(ITelegramBotClient client, Update update, CancellationToken cancellationToken)
    {
        try
        {
            HandleBotJoinedOrLeft(client, update, cancellationToken).GetAwaiter().GetResult();

            // Update must be a valid message
            if (update.Message?.Text is null)
                return Task.CompletedTask;

            if (update.Message.Chat.Type == ChatType.Group || update.Message.Chat.Type == ChatType.Supergroup)
            {
                ResolveTriggersAsync(client, update.Message.Text, update.Message.Chat.Id, update.Message.MessageId, cancellationToken);
                ResolveIllegalNotificationsAsync(client, update, cancellationToken);
            }

            if (update.Message?.From is null)
                return Task.CompletedTask;

            IOHandler.RegisterClient(update.Message.From.Id, update.Message.From.Username, update.Message.From.FirstName);

            // Check if message is a command
            if (!IsValidCommand(update))
                return Task.CompletedTask;

            var type = typeof(WarnController);
            var method = Tools.ResolveMethod(type, update.Message.EntityValues.First()[1..]);

            if (method is null)
                return Task.CompletedTask;

            var response = (BotResponse)(method.Invoke(Activator.CreateInstance(type, null),
                           new object[] { client, update, cancellationToken }) ?? "Executed!");

            switch (response.Type)
            {
                // Succes => delete message and than send the response to chat
                case ResponseType.Succes:
                    if (IOHandler.GetConfiguration().DeleteWarnMessage)
                        client.DeleteMessageAsync(update.Message.Chat.Id, update.Message.MessageId, cancellationToken);
                    goto case ResponseType.Error;

                case ResponseType.Error:
                    client.SendTextMessageAsync(update.Message.Chat.Id, response.Data, cancellationToken: cancellationToken, parseMode: ParseMode.Markdown);
                    break;

                case ResponseType.Unhandled:
                    Console.WriteLine("ResponseType.Unhandled: " + response.Data);
                    break;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("HandleUpdateAsync: " + e.Message);
        }

        return Task.CompletedTask;
    }

    private static Task HandleBotJoinedOrLeft(ITelegramBotClient client, Update update, CancellationToken cancellationToken)
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
            var chats = IOHandler.GetWarnings().Chats;
            chats.RemoveAll(c => c.Id == update.Message.Chat.Id);
        }

        return Task.CompletedTask;
    }

    private static Task ResolveTriggersAsync(ITelegramBotClient client, string message, long chatId, int messageId, CancellationToken cancellationToken)
    {
        return Task.Run(() =>
        {
            foreach (var trigger in IOHandler.GetConfiguration().Triggers)
            {
                if (trigger.Chat is not null && trigger.Chat != chatId)
                    continue;

                if (MatchMessage(trigger.Messages, trigger.MatchWholeMessage, trigger.MatchCase, message))
                {
                    client.SendTextMessageAsync(chatId,
                                                trigger.Response,
                                                replyToMessageId: messageId,
                                                cancellationToken: cancellationToken,
                                                parseMode: ParseMode.Markdown);
                    return;
                }
            }
        }, cancellationToken);
    }

    private static Task ResolveIllegalNotificationsAsync(ITelegramBotClient client, Update update, CancellationToken cancellationToken)
    {
        return Task.Run(async () =>
        {
            foreach (var notification in IOHandler.GetConfiguration().IllegalNotifications)
            {
                if (notification.Chat is not null && notification.Chat != update.Message.Chat.Id)
                    continue;

                if (MatchMessage(notification.IllegalWords, false, false, update.Message.Text))
                {
                    foreach (var adminId in notification.NotifiedAdmins)
                    {
                        await client.SendTextMessageAsync(adminId,
                                                        $"*Illegal message detected!*\nChat: *{update.Message.Chat.Title}*\nFrom: *{update.Message.From?.FirstName}*\nSent: {update.Message.Date}\nContent:",
                                                        cancellationToken: cancellationToken,
                                                        parseMode: ParseMode.Markdown);

                        await client.ForwardMessageAsync(adminId, update.Message.Chat.Id, update.Message.MessageId, cancellationToken: cancellationToken);
                    }
                }
            }
        }, cancellationToken);
    }

    private static bool MatchMessage(string[] matchFromMessages, bool matchWholeMessage, bool matchCase, string message)
    {
        if (matchWholeMessage)
            return matchFromMessages.Any(m => matchCase ? m == message : m.ToLower() == message.ToLower());

        return matchFromMessages.Any(m => message.Contains(m, matchCase ? StringComparison.CurrentCulture : StringComparison.CurrentCultureIgnoreCase));
    }

    private static bool IsValidCommand(Update update)
    {
        return update.Message is not null
            && update.Message.Text is not null
            && update.Message.From is not null
            && (update.Message.Chat.Type == ChatType.Group || update.Message.Chat.Type == ChatType.Supergroup)
            && update.Message.Entities is not null
            && update.Message.EntityValues is not null
            && update.Message.Entities.Length > 0
            && update.Message.Entities[0].Type == MessageEntityType.BotCommand;
    }
}
