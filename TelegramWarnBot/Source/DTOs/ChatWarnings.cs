namespace TelegramWarnBot;

public class ChatWarnings
{
    public long ChatId { get; set; }
    public List<WarnedUser> WarnedUsers { get; set; }
}
