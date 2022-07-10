namespace TelegramWarnBot;

public enum ResolveMentionedUserResult
{
    UserNotMentioned,
    UserNotFound,
    BotMention,
    BotSelfMention,
}

public class BotResponse
{
    public string Data { get; }

    public BotResponse(string data)
    {
        Data = data;
    }
}
