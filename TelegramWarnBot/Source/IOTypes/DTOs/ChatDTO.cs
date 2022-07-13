namespace TelegramWarnBot;

public class ChatDTO
{
    public long Id { get; set; }
    public string Name { get; set; }
    public List<long> Admins { get; set; }
}
