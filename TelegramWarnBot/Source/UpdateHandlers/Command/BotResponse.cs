namespace TelegramWarnBot;

public enum ResolveMentionedUserResult
{
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
