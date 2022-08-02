namespace TelegramWarnBot.Tests;

public class CommandsControllerTests
{
    private readonly CommandsController _sut;

    private readonly MockedUpdateContextBuilder updateContextBuilder = new MockedUpdateContextBuilder();

    private readonly ITelegramBotClientProvider telegramBotClientProvider = Substitute.For<ITelegramBotClientProvider>();
    private readonly IChatHelper chatHelper = Substitute.For<IChatHelper>();
    private readonly IResponseHelper responseHelper = Substitute.For<IResponseHelper>();
    private readonly ICommandService commandService = Substitute.For<ICommandService>();
    private readonly ILogger<CommandsController> logger = Substitute.For<ILogger<CommandsController>>();

    private readonly IFixture fixture = FixtureProvider.Fixture;

    public CommandsControllerTests()
    {
        _sut = new CommandsController(telegramBotClientProvider,
                                      MockedConfigurationContext.Shared,
                                      MockedCachedContext.Shared,
                                      chatHelper,
                                      responseHelper,
                                      commandService,
                                      logger);
    }

    public void Test()
    {
        // todo
    }
}
