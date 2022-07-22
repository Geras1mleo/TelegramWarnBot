namespace TelegramWarnBot;

public interface IBot
{
    TelegramBotClient Client { get; set; }
    User BotUser { get; set; }

    Task StartAsync(IServiceProvider provider, CancellationToken cancellationToken);
}

public class Bot : IBot
{
    private readonly ICachedDataContext cachedContext;
    private readonly IUpdateContextBuilder updateContextBuilder;
    private readonly IConfigurationContext configContext;
    private readonly ILogger<Bot> logger;
    private Func<UpdateContext, Task> pipe;

    public Bot(IConfigurationContext configContext,
               ICachedDataContext cachedContext,
               IUpdateContextBuilder updateContextBuilder,
               ILogger<Bot> logger)
    {
        this.configContext = configContext;
        this.cachedContext = cachedContext;
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
        cachedContext.CacheUser(BotUser);
        cachedContext.BeginUpdate(configContext.Configuration.UpdateDelay, cancellationToken);

        logger.LogInformation("Bot {botName} running.", BotUser.FirstName);
        logger.LogInformation("Version: {version}", Assembly.GetEntryAssembly().GetName().Version);

        Console.Title = BotUser.FirstName;
    }

    private void StartReceiving(IServiceProvider provider, CancellationToken cancellationToken)
    {
        var builder = new PipeBuilder<UpdateContext>(_ => Task.CompletedTask, provider)
                             .AddPipe<JoinedLeftHandler>(c => c.IsJoinedLeftUpdate)
                             .AddPipe<CachingHandler>(c => c.IsMessageUpdate)
                             .AddPipe<AdminsHandler>(c => c.IsAdminsUpdate)
                             .AddPipe<SpamHandler>(c => c.IsBotAdmin && !c.IsSenderAdmin && configContext.Configuration.DeleteLinksFromNewMembers)
                             .AddPipe<TriggersHandler>()
                             .AddPipe<IllegalTriggersHandler>(c => c.IsBotAdmin)
                             .AddPipe<CommandHandler>(c => c.Update.Message.Text.IsValidCommand());

        pipe = builder.Build();

        //logger.LogInformation("Update handlers: {pipes}", builder.GetPipes().Select(p => p.Type.Name));

        Client = new(configContext.BotConfiguration.Token);

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
        // Update must be a valid message with a From-user
        if (!update.Validate())
            return Task.CompletedTask;

        var context = updateContextBuilder.Build(client, update, BotUser, cancellationToken);

        try
        {
            return pipe(context);
        }
        catch (Exception e)
        {
            // Update that raised exception will be saved in Logs.json
            // Bot will skip this message, he wont handle it ever again
            logger.LogError(e, "HandlePollingErrorAsync {@update}", update);
            return Task.CompletedTask;
        }
    }

    private Task PollingErrorHandler(ITelegramBotClient client, Exception exception, CancellationToken cancellationToken)
    {
        logger.LogCritical(exception, "Restart required!");
        return Task.CompletedTask;
    }
}
