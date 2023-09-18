namespace TelegramWarnBot;

public interface IStatsController
{
    void AddDeletedSpamMessage(DeletedMessageLog log);
    void StartTrace(CancellationToken cancellationToken);
}

public class StatsController : IStatsController
{
    private readonly List<DeletedMessageLog> logs = new();

    private readonly ILogger<StatsController> logger;
    private readonly IConfigurationContext configurationContext;

    public StatsController(ILogger<StatsController> logger, IConfigurationContext configurationContext)
    {
        this.logger = logger;
        this.configurationContext = configurationContext;
    }

    public void AddDeletedSpamMessage(DeletedMessageLog log)
    {
        logs.Add(log);
    }

    public void StartTrace(CancellationToken cancellationToken)
    {
        Task.Run(async () =>
        {
            logger.LogInformation("Sending stats each {seconds} hours", configurationContext.Configuration.StatsDelay);
            while (true)
            {
                await Task.Delay(configurationContext.Configuration.StatsDelay * 1000 * 60 * 60, cancellationToken);
                logger.LogTrace("Deleted {count} spam messages in last {hours} hours", logs.Count, configurationContext.Configuration.StatsDelay);
                logs.Clear();
            }
        }, cancellationToken);
    }
}