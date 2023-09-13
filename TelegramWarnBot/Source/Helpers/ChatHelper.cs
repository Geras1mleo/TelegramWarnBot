namespace TelegramWarnBot;

public interface IChatHelper
{
    bool IsChatRegistered(long chatId);
    bool IsAdmin(long chatId, long userId);
    Task<List<long>> GetAdminsAsync(long chatId, long botId, CancellationToken cancellationToken);
}

public class ChatHelper : IChatHelper
{
    private readonly ICachedDataContext cachedDataContext;
    private readonly IConfigurationContext configurationContext;
    private readonly ITelegramBotClientProvider telegramBotClientProvider;

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
        return cachedDataContext.FindChatById(chatId)?.Admins.Any(a => a == userId) ?? false;
    }

    public async Task<List<long>> GetAdminsAsync(long chatId, long botId, CancellationToken cancellationToken)
    {
        var admins = await telegramBotClientProvider.GetChatAdministratorsAsync(chatId, cancellationToken);

        // Adding bot to list of admins only when bot can delete messages and restrict members
        foreach (var admin in admins)
            if (admin.User.Id == botId)
            {
                if (admin is ChatMemberAdministrator chatAdministrator)
                    if (!chatAdministrator.CanDeleteMessages
                     || !chatAdministrator.CanRestrictMembers)
                        return admins.Select(member => member.User.Id).Where(id => id != botId).ToList();
                break;
            }

        return admins.Select(member => member.User.Id).ToList();
    }
}