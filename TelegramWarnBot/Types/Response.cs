namespace TelegramWarnBot;

public enum ResponseType
{
    Succes,
    Error,
    Unhandled,
}

public class BotResponse
{
    public ResponseType Type { get; }
    public string Data { get; }

    public BotResponse(ResponseType type, string data)
    {
        Type = type;
        Data = data;
    }
}
