namespace TelegramWarnBot;

public interface IBot
{
    TelegramBotClient Client { get; set; }
    User User { get; set; }

    Bot Start(ILifetimeScope scope, CancellationToken cancellationToken);
}

public class Bot : IBot
{
    private readonly ICachedDataContext cachedContext;
    private readonly IConfigurationContext configContext;
    private Func<UpdateContext, Task> pipe;

    public Bot(IConfigurationContext configContext,
               ICachedDataContext cachedContext)
    {
        this.configContext = configContext;
        this.cachedContext = cachedContext;
    }

    public TelegramBotClient Client { get; set; }
    public User User { get; set; }

    public Bot Start(ILifetimeScope scope, CancellationToken cancellationToken)
    {
        StartReceiving(scope, cancellationToken);

        // Register bot itself to recognize when someone mentions it with @
        cachedContext.CacheUser(User);
        cachedContext.BeginUpdate(configContext.Configuration.UpdateDelay, cancellationToken);

        Tools.WriteColor($"\n\nBot: [{User.GetFullName()}] running...", ConsoleColor.Green, true);
        Tools.WriteColor($"\n[Version: {Assembly.GetEntryAssembly().GetName().Version}]", ConsoleColor.Yellow, false);

        Console.Title = User.GetFullName();

        return this;
    }

    private void StartReceiving(ILifetimeScope scope, CancellationToken cancellationToken)
    {
        var builder = new PipeBuilder<UpdateContext>(_ => Task.CompletedTask, scope)
                             .AddPipe<JoinedLeftHandler>(c => c.IsJoinedLeftUpdate)
                             .AddPipe<CachingHandler>(c => c.IsChatRegistered && c.IsMessageUpdate)
                             .AddPipe<AdminsHandler>(c => c.IsChatRegistered && c.IsAdminsUpdate)
                             .AddPipe<SpamHandler>(c => c.IsChatRegistered && c.IsBotAdmin && !c.IsSenderAdmin 
                                                     && c.IsText && configContext.Configuration.DeleteLinksFromNewMembers)
                             .AddPipe<TriggersHandler>(c => c.IsChatRegistered && c.IsText)
                             .AddPipe<IllegalTriggersHandler>(c => c.IsChatRegistered && c.IsBotAdmin && c.IsText)
                             .AddPipe<CommandHandler>(c => c.IsText && c.Update.Message.Text.IsValidCommand());

        pipe = builder.Build();

        Client = new(configContext.BotConfiguration.Token);

        User = Client.GetMeAsync(cancellationToken).GetAwaiter().GetResult();

        Client.StartReceiving(UpdateHandler, PollingErrorHandler,
        receiverOptions: new ReceiverOptions()
        {
            AllowedUpdates = new[] { UpdateType.Message, UpdateType.ChatMember, UpdateType.MyChatMember },
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
                        (update.Message.Type == MessageType.ChatMembersAdded || update.Message.Type == MessageType.ChatMemberLeft),
            IsAdminsUpdate = (update.Type == UpdateType.ChatMember || update.Type == UpdateType.MyChatMember) 
                     && (update.GetOldMember().Status == ChatMemberStatus.Administrator || update.GetNewMember().Status == ChatMemberStatus.Administrator),
            IsChatRegistered = configContext.IsChatRegistered(chatId),
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
            cachedContext.Logs.Add(new()
            {
                Update = update,
                Time = DateTime.Now,
                Exception = e.Map()
            });

            Tools.WriteColor($"[HandlePollingErrorAsync]\n[Message]: {e.Message}\n[StackTrace]: {e.StackTrace}", ConsoleColor.Red, true);

            return Task.CompletedTask;
        }
    }

    private Task PollingErrorHandler(ITelegramBotClient client, Exception exception, CancellationToken cancellationToken)
    {
        Tools.WriteColor("[Some error occured! Restart required!]", ConsoleColor.Red, true);
        return Task.CompletedTask;
    }
}
