namespace TelegramWarnBot;

public class Trigger
{
    public long? Chat { get; set; }
    public string[] Messages { get; set; }
    public string[] Responses { get; set; }
    public bool MatchCase { get; set; }
    public bool MatchWholeMessage { get; set; }
}