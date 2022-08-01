namespace TelegramWarnBot;

public interface IChatHelper
{
    bool IsChatRegistered(long chatId);
    bool IsAdmin(long chatId, long userId);
    Task<List<long>> GetAdminsAsync(long chatId, CancellationToken cancellationToken);
}

public class ChatHelper : IChatHelper
{
    private readonly ITelegramBotClientProvider telegramBotClientProvider;
    private readonly ICachedDataContext cachedDataContext;
    private readonly IConfigurationContext configurationContext;

    public ChatHelper(ITelegramBotClientProvider telegramBotClientProvider,
                      ICachedDataContext cachedDataContext,
                      IConfigurationContext configurationContext)
    {
        this.telegramBotClientProvider = telegramBotClientProvider;
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

    public async Task<List<long>> GetAdminsAsync(long chatId, CancellationToken cancellationToken)
    {
        return (await telegramBotClientProvider.Client.GetChatAdministratorsAsync(chatId, cancellationToken))
                                                        .Select(c => c.User.Id).ToList();
    }
}
