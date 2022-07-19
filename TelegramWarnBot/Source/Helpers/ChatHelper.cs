namespace TelegramWarnBot;

public interface IChatHelper
{
    bool IsChatRegistered(long chatId);
    bool IsAdmin(long chatId, long userId);
    Task<List<long>> GetAdminsAsync(UpdateContext context);
}

public class ChatHelper : IChatHelper
{
    private readonly ICachedDataContext cachedDataContext;
    private readonly IConfigurationContext configurationContext;

    public ChatHelper(ICachedDataContext cachedDataContext,
                      IConfigurationContext configurationContext)
    {
        this.cachedDataContext = cachedDataContext;
        this.configurationContext = configurationContext;
    }

    public bool IsChatRegistered(long chatId)
    {
        return configurationContext.BotConfiguration.RegisteredChats.Any(c => c == chatId);
    }

    public bool IsAdmin(long chatId, long userId)
    {
        return cachedDataContext.Chats.Find(c => c.Id == chatId)?.Admins.Any(a => a == userId) ?? false;
    }

    public async Task<List<long>> GetAdminsAsync(UpdateContext context)
    {
        return (await context.Client.GetChatAdministratorsAsync(context.ChatDTO.Id, context.CancellationToken)).Select(c => c.User.Id).ToList();
    }
}
