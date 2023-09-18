namespace TelegramWarnBot;

public class TelegramInfoSink : ILogEventSink
{
    private readonly long[] receivers;

    public TelegramInfoSink(long[] receivers)
    {
        this.receivers = receivers;
    }

    public void Emit(LogEvent logEvent)
    {
        if (receivers is null
         || logEvent.Level != LogEventLevel.Verbose
         || !logEvent.Properties.TryGetValue("count", out _))
            return;

        foreach (var clientId in receivers)
        {
            try
            {
                TelegramBotClientProvider.Shared.SendMessageAsync(clientId, logEvent.RenderMessage())
                    .GetAwaiter().GetResult();
            }
            catch
            {
                // ignored
            }
        }
    }
}