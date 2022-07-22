namespace TelegramWarnBot;

public enum ResolveMentionedUserResult
{
    Resolved,
    UserNotMentioned,
    UserNotFound,
    BotMention,
    BotSelfMention,
}

public class ResponseContext
{
    public string Message { get; set; }
    public long? MentionedUserId { get; set; }
}
