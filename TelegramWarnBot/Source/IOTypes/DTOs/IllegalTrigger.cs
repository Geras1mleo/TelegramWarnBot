namespace TelegramWarnBot;

public class IllegalTrigger
{
    public long? Chat { get; set; }
    public bool WarnMember { get; set; }
    public bool DeleteMessage { get; set; }
    public bool IgnoreAdmins { get; set; }
    public string[] IllegalWords { get; set; }
    public long[] NotifiedAdmins { get; set; }
}