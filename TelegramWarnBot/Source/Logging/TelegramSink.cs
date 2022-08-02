namespace TelegramWarnBot;

public class TelegramSink : ILogEventSink
{
    private readonly long[] notifyBotOwners;

    public TelegramSink(long[] notifyBotOwners)
    {
        this.notifyBotOwners = notifyBotOwners;
    }

    public void Emit(LogEvent logEvent)
    {
        if (notifyBotOwners is null)
            return;

        logEvent.RemovePropertyIfPresent("update");

        string message = logEvent.RenderMessage();

        try
        {
            foreach (var userId in notifyBotOwners)
            {
                TelegramBotClientProvider.Shared.SendMessageAsync(userId, $"{logEvent.Level}: " + message)
                                                       .GetAwaiter().GetResult();
            }
        }
        catch (Exception) { }
    }
}

public static class TelegramSinkExtensions
{
    public static LoggerConfiguration TelegramSink(
        this LoggerSinkConfiguration loggerConfiguration,
        long[] notifyBotOwners = null,
        LogEventLevel restrictedToMinimumLevel = LogEventLevel.Warning)
    {
        return loggerConfiguration.Sink(new TelegramSink(notifyBotOwners), restrictedToMinimumLevel);
    }
}