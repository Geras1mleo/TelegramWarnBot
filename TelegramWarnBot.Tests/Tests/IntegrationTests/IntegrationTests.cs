namespace TelegramWarnBot.Tests;

public class IntegrationTestsBase
{
    protected readonly MockedConfigurationContext mockedConfigurationContext = new();
    protected readonly MockedCachedContext mockedCachedContext = new();
    protected readonly MockedInMemoryCachedContext mockedInMemoryCachedContext = new();

    protected readonly ITelegramBotClientProvider clientProvider = Substitute.For<ITelegramBotClientProvider>();
    protected readonly IBot bot;

    public IntegrationTestsBase()
    {
        // todo report issue
        //client.DeleteMessageAsync(default, default).ReturnsForAnyArgs(Task.CompletedTask);
        //client.BanChatMemberAsync(default, default).ReturnsForAnyArgs(Task.CompletedTask);
        //client.UnbanChatMemberAsync(default, default).ReturnsForAnyArgs(Task.CompletedTask);

        clientProvider.SendMessageAsync(default, default, default, default).ReturnsForAnyArgs(Task.FromResult(new Message()));
        clientProvider.DeleteMessageAsync(default, default, default).ReturnsForAnyArgs(Task.CompletedTask);
        clientProvider.BanChatMemberAsync(default, default, default).ReturnsForAnyArgs(Task.CompletedTask);
        clientProvider.UnbanChatMemberAsync(default, default, default).ReturnsForAnyArgs(Task.CompletedTask);
        clientProvider.GetChatAdministratorsAsync(default, default).ReturnsForAnyArgs(Task.FromResult(new ChatMember[] { new ChatMemberMember() }));
        clientProvider.ForwardMessageAsync(default, default, default, default).ReturnsForAnyArgs(Task.FromResult(new Message()));
        clientProvider.GetMeAsync(default).ReturnsForAnyArgs(Task.FromResult(new User() { Id = 99, Username = "warn_bot", FirstName = "Moderator" }));

        var servicesCollection = new ServiceCollection()
            .AddSingleton(clientProvider)
            .AddSingleton<IBot, Bot>()
            .AddSingleton<IConfigurationContext>(mockedConfigurationContext)
            .AddSingleton<ICachedDataContext>(mockedCachedContext)
            .AddSingleton<IInMemoryCachedDataContext>(mockedInMemoryCachedContext)
            .AddTransient<IUpdateContextBuilder, UpdateContextBuilder>()
            .AddTransient<IDateTimeProvider, DateTimeProvider>()
            .AddTransient<ICommandsController, CommandsController>()
            .AddTransient<IMessageHelper, MessageHelper>()
            .AddTransient<IChatHelper, ChatHelper>()
            .AddTransient<ICommandService, CommandService>()
            .AddTransient<IResponseHelper, ResponseHelper>()
            .AddSmartFormatterProvider()
            .AddSingleton(Substitute.For<ILogger<Bot>>())
            .AddSingleton(Substitute.For<ILogger<AdminsHandler>>())
            .AddSingleton(Substitute.For<ILogger<CommandHandler>>())
            .AddSingleton(Substitute.For<ILogger<IllegalTriggersHandler>>())
            .AddSingleton(Substitute.For<ILogger<JoinedLeftHandler>>())
            .AddSingleton(Substitute.For<ILogger<SpamHandler>>())
            .AddSingleton(Substitute.For<ILogger<TriggersHandler>>())
            .AddSingleton(Substitute.For<ILogger<CommandsController>>());

        var services = servicesCollection.BuildServiceProvider();

        bot = services.GetService<IBot>();
        bot.StartAsync(CancellationToken.None).GetAwaiter().GetResult();
    }
}
