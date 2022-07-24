
// A wise man once said:"If your code is hard to test, u wrote bad code, my friend..."
// Soo.. my code is just awful

namespace TelegramWarnBot.Tests;

public class TriggersHandlerTests
{
    private readonly TriggersHandler _sut;

    private readonly MockedUpdateContextBuilder updateContextBuilder = new MockedUpdateContextBuilder();

    private readonly IResponseHelper responseHelper = Substitute.For<IResponseHelper>();

    private readonly IFixture fixture = FixtureProvider.Fixture;

    public TriggersHandlerTests()
    {
        _sut = new TriggersHandler(c => Task.CompletedTask,
                                   MockedConfigurationContext.Shared,
                                   MessageHelperProvider.MessageHelper,
                                   responseHelper);
    }

    [Theory]
    [InlineData("good russians", true)]
    [InlineData("GOOD russIANS", true)]
    [InlineData("bla bla bla GOOD russIANS bla bla", true)]
    [InlineData("just a message with good russians", true)]
    [InlineData("not like this russians...", false)]
    [InlineData("just a message", false)]
    [InlineData("bad russians", false)]
    [InlineData("pidor", true)]
    [InlineData("Pidor", false)]
    [InlineData("pidor writing", false)]
    public async Task Handle_ShouldSendMessage_WhenTriggerIsInvoked(string message, bool triggered)
    {
        // Arrange
        var update = fixture.BuildMessageUpdate(message);
        var context = updateContextBuilder.BuildMocked(update);

        responseHelper.SendMessageAsync(Arg.Any<ResponseContext>(),
                                        Arg.Any<UpdateContext>(),
                                        Arg.Is(context.Update.Message.MessageId))
                      .Returns(Task.CompletedTask);

        // Act
        await _sut.Handle(context);

        // Assert
        await responseHelper.Received(triggered ? 1 : 0)
                            .SendMessageAsync(Arg.Any<ResponseContext>(),
                                              Arg.Any<UpdateContext>(),
                                              Arg.Is(context.Update.Message.MessageId));
    }
}
