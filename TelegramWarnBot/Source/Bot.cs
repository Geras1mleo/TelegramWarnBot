namespace TelegramWarnBot;

public interface IBot
{
    TelegramBotClient Client { get; set; }
    User User { get; set; }

    Task StartAsync(IServiceProvider provider, CancellationToken cancellationToken);
}

public class Bot : IBot
{
    private readonly ICachedDataContext cachedContext;
    private readonly IConfigurationContext configContext;
    private readonly IChatHelper chatHelper;
    private readonly ILogger<Bot> logger;
    private Func<UpdateContext, Task> pipe;

    public Bot(IConfigurationContext configContext,
               ICachedDataContext cachedContext,
               IChatHelper chatHelper,
               ILogger<Bot> logger)
    {
        this.configContext = configContext;
        this.cachedContext = cachedContext;
        this.chatHelper = chatHelper;
        this.logger = logger;
    }

    public TelegramBotClient Client { get; set; }
    public User User { get; set; }

    public async Task StartAsync(IServiceProvider provider, CancellationToken cancellationToken)
    {
        StartReceiving(provider, cancellationToken);

        User = await Client.GetMeAsync(cancellationToken);

        // Register bot itself to recognize when someone mentions it with @
        cachedContext.CacheUser(User);
        cachedContext.BeginUpdate(configContext.Configuration.UpdateDelay, cancellationToken);

        logger.LogInformation("Bot {botName} running.", User.FirstName);
        logger.LogInformation("Version: {version}", Assembly.GetEntryAssembly().GetName().Version);

        Console.Title = User.FirstName;
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

        var chatId = update.GetChat().Id;

        var chatDto = cachedContext.Chats.Find(c => c.Id == chatId);
        
        var fromUser = update.GetFromUser();

        var context = new UpdateContext
        {
            Client = Client,
            Update = update,
            CancellationToken = cancellationToken,
            Bot = User,
            ChatDTO = chatDto,
            IsMessageUpdate = update.Type == UpdateType.Message,
            IsText = update.Message?.Text is not null,
            IsJoinedLeftUpdate = update.Type == UpdateType.Message &&
                                    (update.Message.Type == MessageType.ChatMembersAdded
                                  || update.Message.Type == MessageType.ChatMemberLeft),
            IsAdminsUpdate = (update.Type == UpdateType.ChatMember
                            || update.Type == UpdateType.MyChatMember)
                          && (update.GetOldMember().Status == ChatMemberStatus.Administrator
                            || update.GetNewMember().Status == ChatMemberStatus.Administrator),
            IsChatRegistered = chatHelper.IsChatRegistered(chatId),
            IsBotAdmin = chatDto?.Admins.Any(a => a == User.Id) ?? false,
            IsSenderAdmin = chatDto?.Admins.Any(a => a == fromUser.Id) ?? false,
        };

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
