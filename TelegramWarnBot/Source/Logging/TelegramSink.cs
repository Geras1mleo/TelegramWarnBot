namespace TelegramWarnBot;

public class TelegramSink : ILogEventSink
{
    private readonly long[] notifyBotOwners;

    public TelegramSink(long[] notifyBotOwners)
    {
        // g: 402649130 todo
        this.notifyBotOwners = notifyBotOwners;
    }

    public void Emit(LogEvent logEvent)
    {
        if (notifyBotOwners is null)
            return;

        try
        {
            foreach (var userId in notifyBotOwners)
            {
                TelegramBotClientProvider.Shared.Client.SendTextMessageAsync(userId, $"{logEvent.Level}: " + logEvent.Exception.Message).GetAwaiter().GetResult();
            }
        }
        catch (Exception) { }
    }
}

public static class TelegramSinkExtensions
{
    public static LoggerConfiguration TelegramSink(this LoggerSinkConfiguration loggerConfiguration,
        long[] notifyBotOwners = null,
        LogEventLevel restrictedToMinimumLevel = LogEventLevel.Warning)
    {
        return loggerConfiguration.Sink(new TelegramSink(notifyBotOwners), restrictedToMinimumLevel);
    }
}