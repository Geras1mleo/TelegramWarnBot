namespace TelegramWarnBot;

public class IllegalTrigger
{
    public long? Chat { get; set; }
    public string[] IllegalWords { get; set; }
    public long[] NotifiedAdmins { get; set; }
    public bool WarnMember { get; set; }
}
