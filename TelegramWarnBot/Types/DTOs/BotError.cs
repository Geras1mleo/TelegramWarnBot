namespace TelegramWarnBot;

public class BotError
{
    public DateTime Time { get; set; }
    public Exception Exception { get; set; }
    public Update Update { get; set; }
}
