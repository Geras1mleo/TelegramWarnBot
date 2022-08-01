namespace TelegramWarnBot;

public interface IBot
{
    User BotUser { get; }

    Task StartAsync(CancellationToken cancellationToken);
}

public class Bot : IBot
{
    private readonly IServiceProvider serviceProvider;
    private readonly ITelegramBotClientProvider telegramBotClientProvider;
    private readonly IConfigurationContext configurationContext;
    private readonly ICachedDataContext cachedDataContext;
    private readonly IUpdateContextBuilder updateContextBuilder;
    private readonly ILogger<Bot> logger;

    private Func<UpdateContext, Task> pipe;

    public User BotUser { get; private set; }

    public Bot(IServiceProvider serviceProvider,
               ITelegramBotClientProvider telegramBotClientProvider,
               IConfigurationContext configurationContext,
               ICachedDataContext cachedDataContext,
               IUpdateContextBuilder updateContextBuilder,
               ILogger<Bot> logger)
    {
        this.serviceProvider = serviceProvider;
        this.telegramBotClientProvider = telegramBotClientProvider;
        this.configurationContext = configurationContext;
        this.cachedDataContext = cachedDataContext;
        this.updateContextBuilder = updateContextBuilder;
        this.logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        StartReceiving(cancellationToken);

        BotUser = await telegramBotClientProvider.Client.GetMeAsync(cancellationToken);

        // Register bot itself to recognize when someone mentions it with @
        cachedDataContext.CacheUser(BotUser);
        cachedDataContext.BeginUpdate(configurationContext.Configuration.UpdateDelay, cancellationToken);

        logger.LogInformation("Bot {botName} running.", BotUser.FirstName);
        logger.LogInformation("Version: {version}", Assembly.GetEntryAssembly().GetName().Version);

        Console.Title = BotUser.FirstName;
    }

    private void StartReceiving(CancellationToken cancellationToken)
    {
        pipe = AppConfiguration.GetPipeBuilder(serviceProvider).Build();

        logger.LogInformation("Starting receiving updates");

        telegramBotClientProvider.Client.StartReceiving(UpdateHandler, PollingErrorHandler,
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

            var context = updateContextBuilder.Build(update, BotUser, cancellationToken);

            return pipe(context);
        }
        catch (Exception exception)
        {
            // Update that raised exception will be saved in Logs.json (and sent to tech support in private messages)
            logger.LogError(exception, "Handler error on update {@update} in chat {chat}", update, update.GetChat()?.Title);
            return Task.CompletedTask;
        }
    }

    private Task PollingErrorHandler(ITelegramBotClient client, Exception exception, CancellationToken cancellationToken)
    {
        logger.LogCritical(exception, "Fatal error occured. Restart required!");
        return Task.CompletedTask;
    }
}
