

namespace TelegramWarnBot;

public static class BotHandlers
{
    public static User MeUser { get; set; } = null;

    public static Task HandlePollingErrorAsync(ITelegramBotClient client, Exception exception, CancellationToken cancellationToken) => Task.CompletedTask;

    public static Task HandleUpdateAsync(ITelegramBotClient client, Update update, CancellationToken cancellationToken)
    {
        // todo generate info /start in private start up
        try
        {
            if (update.Message?.From is null)
                return Task.CompletedTask;

            IOHandler.RegisterClientAsync(update.Message.From.Id, update.Message.From.Username, update.Message.From.FirstName);

            if (!IsValidCommand(update))
                return Task.CompletedTask;

            var type = typeof(WarnController);
            var method = ResolveMethod(type, update.Message.EntityValues.First()[1..]);

            if (method is not null)
            {
                var response = (BotResponse)(method.Invoke(Activator.CreateInstance(type, null),
                               new object[] { client, update, cancellationToken }) ?? "Executed!");

                switch (response.Type)
                {
                    // In Succes deletes message and than send the response to chat
                    case ResponseType.Succes:
                        client.DeleteMessageAsync(new ChatId(update.Message.Chat.Id), update.Message.MessageId, cancellationToken);
                        goto case ResponseType.Error;
                    case ResponseType.Error:
                        client.SendTextMessageAsync(update.Message.Chat.Id, response.Data, cancellationToken: cancellationToken, parseMode: ParseMode.Markdown);
                        break;
                    case ResponseType.Unhandled:
                        Console.WriteLine("ResponseType.Unhandled: " + response.Data);
                        break;
                }

            }
        }
        catch (Exception e)
        {
            Console.WriteLine("HandleUpdateAsync: " + e.Message);
        }

        return Task.CompletedTask;
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

    private static MethodInfo ResolveMethod(Type type, string prefix)
    {
        var members = type.GetMethods();
        return members.FirstOrDefault(m => m.Name.ToLower().Equals(prefix));
    }
}
