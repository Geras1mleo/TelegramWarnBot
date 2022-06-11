namespace TelegramWarnBot;

public class Configuration
{
    public int UpdateDelay { get; set; }
    public int MaxWarnings { get; set; }
    public bool DeleteWarnMessage { get; set; }
    public bool AllowAdminWarnings { get; set; }
    public Captions Captions { get; set; }
}
