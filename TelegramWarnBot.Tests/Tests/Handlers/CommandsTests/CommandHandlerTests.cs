namespace TelegramWarnBot;

public class CommandHandlerTests
{
    private readonly CommandHandler _sut;

    private readonly MockedUpdateContextBuilder updateContextBuilder = new MockedUpdateContextBuilder();
    private readonly ICommandsController warnController = Substitute.For<ICommandsController>();
    private readonly IResponseHelper responseHelper = Substitute.For<IResponseHelper>();
    private readonly ILogger<CommandHandler> logger = Substitute.For<ILogger<CommandHandler>>();

    private readonly IFixture fixture = FixtureProvider.Fixture;

    public CommandHandlerTests()
    {
        _sut = new CommandHandler(c => Task.CompletedTask,
                                  MockedConfigurationContext.Shared,
                                  warnController,
                                  responseHelper,
                                  logger);
    }

    [Fact]
    public async Task Handle_ShouldSendMessageToChat_WhenNotRegistered()
    {
        // Arrange
        var update = fixture.BuildMessageUpdate("test", 15);

        update.Message.Text = "/warn @robin_thicke";

        var context = updateContextBuilder.BuildMocked(update);

        context.ChatDTO = new()
        {
            Id = 15,
            Name = "test chat"
        };

        responseHelper.SendMessageAsync(Arg.Any<ResponseContext>(), context)
            .Returns(Task.CompletedTask);

        // Act
        await _sut.Handle(context);

        // Assert
        await responseHelper.Received(1).SendMessageAsync(Arg.Any<ResponseContext>(), context);
    }
}
