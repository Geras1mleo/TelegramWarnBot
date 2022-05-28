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

            ResolveTriggers(client, update.Message.Text, update.Message.Chat.Id, update.Message.MessageId, cancellationToken);

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

    private static void ResolveTriggers(ITelegramBotClient client, string message, long chatId, int messageId, CancellationToken cancellationToken)
    {
        foreach (var trigger in IOHandler.GetConfiguration().Triggers)
        {
            if (trigger.MatchWholeMessage)
            {
                if (MatchMessage(trigger.MatchCase, trigger.Message, message))
                {
                    client.SendTextMessageAsync(chatId,
                                                trigger.Response,
                                                replyToMessageId: messageId,
                                                cancellationToken: cancellationToken,
                                                parseMode: ParseMode.Markdown);
                    return;
                }
                continue;
            }

            if (message.Contains(trigger.Message, trigger.MatchCase ? StringComparison.CurrentCulture : StringComparison.CurrentCultureIgnoreCase))
            {
                client.SendTextMessageAsync(chatId,
                                            trigger.Response,
                                            replyToMessageId: messageId,
                                            cancellationToken: cancellationToken,
                                            parseMode: ParseMode.Markdown);
                return;
            }
        }
    }

    private static bool MatchMessage(bool matchCase, string trigger, string message)
    {
        return matchCase ? trigger == message : trigger.ToLower() == message.ToLower();
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
