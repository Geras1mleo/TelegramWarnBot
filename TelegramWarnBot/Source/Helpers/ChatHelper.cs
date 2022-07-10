namespace TelegramWarnBot;

public class ChatHelper
{
    private readonly CachedDataContext cachedDataContext;

    public ChatHelper(CachedDataContext cachedDataContext)
    {
        this.cachedDataContext = cachedDataContext;
    }

    public bool IsAdmin(long chatId, long userId)
    {
        return cachedDataContext.Chats.Find(c => c.Id == chatId)?.Admins.Any(a => a == userId) ?? false;
    }

    public async Task<long[]> GetAdminsAsync(ITelegramBotClient client, long chatId, CancellationToken cancellationToken)
    {
        return (await client.GetChatAdministratorsAsync(chatId, cancellationToken)).Select(c => c.User.Id).ToArray();
    }
}
