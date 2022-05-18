namespace TelegramWarnBot;

public class Configuration
{
    public string Token { get; set; }
    public Responses Captions { get; set; }
    public int UpdateDelay { get; set; }
    public int MaxWarnings { get; set; }
    public string OnBotJoinedChatMessage { get; set; }
}
