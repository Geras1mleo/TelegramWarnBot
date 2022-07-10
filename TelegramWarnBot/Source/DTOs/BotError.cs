namespace TelegramWarnBot;

public class BotError
{
    public DateTime Time { get; set; }
    public SerializableException Exception { get; set; }
    public Update Update { get; set; }
}

public class SerializableException
{
    public string Message { get; set; }
    public string StackTrace { get; set; }
}