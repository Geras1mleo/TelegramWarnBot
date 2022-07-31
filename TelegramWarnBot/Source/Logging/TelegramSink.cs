namespace TelegramWarnBot;

public class TelegramSink : ILogEventSink
{
    public void Emit(LogEvent logEvent)
    {
        try
        {
            Bot.Shared.Client.SendTextMessageAsync(713766114, $"{logEvent.Level}: " + logEvent.Exception.Message).GetAwaiter().GetResult();
            // g: 402649130
        }
        catch (Exception) { }
    }
}

public static class TelegramSinkExtensions
{
    public static LoggerConfiguration TelegramSink(this LoggerSinkConfiguration loggerConfiguration)
    {
        return loggerConfiguration.Sink(new TelegramSink(), LogEventLevel.Error);
    }
}