namespace TelegramWarnBot;

public interface ITelegramBotClientProvider
{
    ITelegramBotClient Client { get; }
}

public class TelegramBotClientProvider : ITelegramBotClientProvider
{
    public static TelegramBotClientProvider Shared { get; private set; } // Only for TelegramSink logging

    public ITelegramBotClient Client { get; }

    public TelegramBotClientProvider(IConfigurationContext configurationContext)
    {
        Client = new TelegramBotClient(configurationContext.BotConfiguration.Token);

        Shared = this;
    }
}