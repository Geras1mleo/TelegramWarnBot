
// A wise man once said:"If your code is hard to test, u wrote bad code, my friend..."
// Soo.. my code is just awful

namespace TelegramWarnBot.Tests;

public class TriggersHandlerTests
{
    private readonly TriggersHandler _sut;

    private readonly IFixture fixture = new Fixture();

    private readonly IConfigurationContext configurationContext = new MockedConfigurationContext();
    private readonly ICachedDataContext cachedDataContext = new MockedCachedContext();

    private readonly IMessageHelper messageHelper = new MessageHelper();

    private readonly UpdateContextBuilder updateContextBuilder;

    private readonly IResponseHelper responseHelper = Substitute.For<IResponseHelper>();

    public TriggersHandlerTests()
    {
        _sut = new TriggersHandler(c => Task.CompletedTask,
                                   configurationContext,
                                   messageHelper,
                                   responseHelper);

        fixture.Behaviors.Remove(new ThrowingRecursionBehavior());
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        fixture.Customizations.Add(new TypeRelay(
                                   typeof(ChatMember),
                                   typeof(ChatMemberMember)));

        updateContextBuilder = new(cachedDataContext, new ChatHelper(cachedDataContext, configurationContext));
    }

    [Theory]
    [InlineData("good russians", true)]
    [InlineData("GOOD russIANS", true)]
    [InlineData("bla bla bla GOOD russIANS bla bla", true)]
    [InlineData("not like this russians...", false)]
    [InlineData("bad russians", false)]
    [InlineData("just a message", false)]
    [InlineData("just a message with good russians", true)]
    [InlineData("pidor", true)]
    [InlineData("pidor writing", false)]
    [InlineData("Pidor", false)]
    public async Task Handle_ShouldSendMessage_WhenTriggerIsInvoked(string message, bool triggered)
    {
        var update = fixture.Build<Update>()
                            .With(u => u.Message, fixture.BuildMessage(message))
                            .Create();

        var context = updateContextBuilder.Build(null, update, fixture.Build<User>().Create(), CancellationToken.None);

        responseHelper.SendMessageAsync(Arg.Any<ResponseContext>(),
                                        Arg.Any<UpdateContext>(),
                                        Arg.Is(context.Update.Message.MessageId))
                                        .Returns(Task.CompletedTask);

        await _sut.Handle(context);

        await responseHelper.Received(triggered ? 1 : 0)
                            .SendMessageAsync(Arg.Any<ResponseContext>(),
                                              Arg.Any<UpdateContext>(),
                                              Arg.Is(context.Update.Message.MessageId));
    }
}
