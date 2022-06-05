namespace TelegramWarnBot;

public class Notification
{
    public string[] IllegalWords { get; set; }
    public long[] NotifiedAdmins { get; set; }
    public long? Chat { get; set; }
}
