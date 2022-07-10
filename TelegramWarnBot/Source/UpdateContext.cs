namespace TelegramWarnBot;

public class UpdateContext
{
    public ITelegramBotClient Client { get; init; }
    public Update Update { get; init; }
    public CancellationToken CancellationToken { get; init; }
    public User Bot { get; init; }
    public ChatDTO ChatDTO { get; init; }
    public bool IsChatRegistered { get; init; }
    public bool IsBotAdmin { get; init; }
    public bool IsSenderAdmin { get; init; }
}
