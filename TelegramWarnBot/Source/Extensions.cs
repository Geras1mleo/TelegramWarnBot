namespace TelegramWarnBot;

public static class Extensions
{
    public static bool Validate(this Update update)
    {
        return update.Message is not null
            && update.Message.From is not null
            && (update.Message.Chat.Type == ChatType.Group || update.Message.Chat.Type == ChatType.Supergroup);
    }

    public static bool IsValidCommand(this string message)
    {
        var parts = message.Split(' ');
        return parts.Length > 0 && parts[0].StartsWith('/');
    }

    public static string GetFullName(this User user)
    {
        return (user.FirstName + " " + user.LastName).Trim();
    }

    public static SerializableException Map(this Exception exception)
    {
        return new()
        {
            Message = exception.Message,
            StackTrace = exception.StackTrace
        };
    }
}
