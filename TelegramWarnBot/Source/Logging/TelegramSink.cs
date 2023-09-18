namespace TelegramWarnBot;

public class TelegramSink : ILogEventSink
{
    private readonly long[] admins;

    public TelegramSink(long[] admins)
    {
        this.admins = admins;
    }

    public void Emit(LogEvent logEvent)
    {
        if (admins is null || logEvent.Exception is RequestException)
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
            foreach (var userId in admins)
                TelegramBotClientProvider.Shared.SendMessageAsync(userId, $"{logEvent.Level}: " + message)
                    .GetAwaiter().GetResult();
        }
        catch
        {
            // ignored
        }
    }
}