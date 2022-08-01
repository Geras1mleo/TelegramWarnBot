namespace TelegramWarnBot;

public interface ITelegramBotClientProvider
{
    TelegramBotClient Client { get; }
}

public class TelegramBotClientProvider : ITelegramBotClientProvider
{
    public static TelegramBotClientProvider Shared { get; private set; } // Only for TelegramSink logging

    public TelegramBotClient Client { get; }

    public TelegramBotClientProvider(IConfigurationContext configurationContext)
    {
        Client = new(configurationContext.BotConfiguration.Token);

        Shared = this;
    }
}