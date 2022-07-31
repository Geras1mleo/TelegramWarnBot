namespace TelegramWarnBot;

public interface IBot
{
    TelegramBotClient Client { get; set; }
    User BotUser { get; set; }

    Task StartAsync(IServiceProvider provider, CancellationToken cancellationToken);
}

public class Bot : IBot
{
    private readonly ICachedDataContext cachedDataContext;
    private readonly IUpdateContextBuilder updateContextBuilder;
    private readonly IConfigurationContext configurationContext;
    private readonly ILogger<Bot> logger;

    private Func<UpdateContext, Task> pipe;

    public Bot(IConfigurationContext configurationContext,
               ICachedDataContext cachedDataContext,
               IUpdateContextBuilder updateContextBuilder,
               ILogger<Bot> logger)
    {
        this.configurationContext = configurationContext;
        this.cachedDataContext = cachedDataContext;
        this.updateContextBuilder = updateContextBuilder;
        this.logger = logger;
    }

    public TelegramBotClient Client { get; set; }
    public User BotUser { get; set; }

    public async Task StartAsync(IServiceProvider provider, CancellationToken cancellationToken)
    {
        StartReceiving(provider, cancellationToken);

        BotUser = await Client.GetMeAsync(cancellationToken);

        // Register bot itself to recognize when someone mentions it with @
        cachedDataContext.CacheUser(BotUser);
        cachedDataContext.BeginUpdate(configurationContext.Configuration.UpdateDelay, cancellationToken);

        logger.LogInformation("Bot {botName} running.", BotUser.FirstName);
        logger.LogInformation("Version: {version}", Assembly.GetEntryAssembly().GetName().Version);

        Console.Title = BotUser.FirstName;
    }

    private void StartReceiving(IServiceProvider provider, CancellationToken cancellationToken)
    {
        pipe = AppConfiguration.GetPipeBuilder(provider).Build();

        Client = new(configurationContext.BotConfiguration.Token);

        Client.StartReceiving(UpdateHandler, PollingErrorHandler,
        receiverOptions: new ReceiverOptions()
        {
            AllowedUpdates = new[]
            {
                UpdateType.Message,
                UpdateType.ChatMember,
                UpdateType.MyChatMember
            },
        },
        cancellationToken: cancellationToken);
    }

    private Task UpdateHandler(ITelegramBotClient client, Update update, CancellationToken cancellationToken)
    {
        try
        {
            // Update must be a valid message with a From-user
            if (!update.Validate())
                return Task.CompletedTask;

            var context = updateContextBuilder.Build(client, update, BotUser, cancellationToken);

            return pipe(context);
        }
        catch (Exception exception)
        {
            // Update that raised exception will be saved in Logs.json (and sent to tech support in private messages)
            logger.LogError(exception, "Handler error on update: {@update}", update);
            return Task.CompletedTask;
        }
    }

    private Task PollingErrorHandler(ITelegramBotClient client, Exception exception, CancellationToken cancellationToken)
    {
        logger.LogCritical(exception, "Fatal error occured... Restart required!");
        return Task.CompletedTask;
    }
}
