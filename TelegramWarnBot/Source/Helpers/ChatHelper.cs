namespace TelegramWarnBot;

public interface IChatHelper
{
    Task<List<long>> GetAdminsAsync(ITelegramBotClient client, long chatId, CancellationToken cancellationToken);
    bool IsAdmin(long chatId, long userId);
}

public class ChatHelper : IChatHelper
{
    private readonly ICachedDataContext cachedDataContext;

    public ChatHelper(ICachedDataContext cachedDataContext)
    {
        this.cachedDataContext = cachedDataContext;
    }

    public bool IsAdmin(long chatId, long userId)
    {
        return cachedDataContext.Chats.Find(c => c.Id == chatId)?.Admins.Any(a => a == userId) ?? false;
    }

    public async Task<List<long>> GetAdminsAsync(ITelegramBotClient client, long chatId, CancellationToken cancellationToken)
    {
        return (await client.GetChatAdministratorsAsync(chatId, cancellationToken)).Select(c => c.User.Id).ToList();
    }
}
