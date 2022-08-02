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

        string message;

        if (logEvent.Properties.TryGetValue("update", out var property))
        {
            logEvent.RemovePropertyIfPresent("update");

            message = logEvent.RenderMessage().Replace(" {@update}", "");

            logEvent.AddPropertyIfAbsent(new LogEventProperty("update", property));
        }
        else
        {
            message = logEvent.RenderMessage();
        }

        // Adding exception details to message
        message += logEvent.Exception?.Message is not null ? "\n" + logEvent.Exception.Message : "";

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