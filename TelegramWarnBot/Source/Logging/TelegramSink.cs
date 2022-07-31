namespace TelegramWarnBot;

public class TelegramSink : ILogEventSink
{
    private readonly IBot bot;

    public TelegramSink(IBot bot)
    {
        this.bot = bot;
    }

    public void Emit(LogEvent logEvent)
    {
        try
        {
            bot.Client.SendTextMessageAsync(713766114, $"{logEvent.Level}: " + logEvent.Exception.Message).GetAwaiter().GetResult();
            // g: 402649130
        }
        catch (Exception) { }
    }
}

public static class TelegramSinkExtensions
{
    public static LoggerConfiguration TelegramSink(this LoggerSinkConfiguration loggerConfiguration, IBot bot)
    {
        return loggerConfiguration.Sink(new TelegramSink(bot), LogEventLevel.Error);
    }
}