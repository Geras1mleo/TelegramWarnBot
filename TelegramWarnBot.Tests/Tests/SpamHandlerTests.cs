namespace TelegramWarnBot.Tests;

public class SpamHandlerTests
{

    private readonly SpamHandler _sut;

    private readonly MockedUpdateContextBuilder updateContextBuilder = new MockedUpdateContextBuilder();

    private readonly IResponseHelper responseHelper = Substitute.For<IResponseHelper>();

    private readonly IDateTimeProvider dateTimeProvider = Substitute.For<IDateTimeProvider>();

    private readonly IFixture fixture = FixtureProvider.Fixture;

    public SpamHandlerTests()
    {
        _sut = new SpamHandler(c => Task.CompletedTask,
                               MockedCachedContext.Shared,
                               MessageHelperProvider.MessageHelper,
                               responseHelper,
                               dateTimeProvider);
    }

    [Theory]
    [InlineData(MessageEntityType.Url, 20, true)]
    [InlineData(MessageEntityType.TextLink, 20, true)]
    [InlineData(MessageEntityType.Mention, 20, true)]
    [InlineData(MessageEntityType.TextMention, 20, true)]
    [InlineData(MessageEntityType.Url, 25, false)]
    [InlineData(MessageEntityType.BotCommand, 20, false)]
    [InlineData(MessageEntityType.Spoiler, 20, false)]
    public async Task Handle_ShouldDeleteMessage_WhenJoinedLessThan24HoursAgo(MessageEntityType type, int hoursAgoJoined, bool deleted)
    {
        // Arrange
        var update = fixture.BuildMessageUpdate("test");

        update.Message.Entities = new MessageEntity[]
        {
            new MessageEntity()
            {
                Type = type
            }
        };

        var context = updateContextBuilder.BuildMocked(update);

        MockedCachedContext.Shared.Members.Clear();
        MockedCachedContext.Shared.Members.Add(new()
        {
            ChatId = 69,
            UserId = 420,
            JoinedDate = DateTime.Now.AddHours(-hoursAgoJoined)
        });

        dateTimeProvider.DateTimeNow.Returns(DateTime.Now);

        responseHelper.DeleteMessageAsync(Arg.Any<UpdateContext>())
                      .Returns(Task.CompletedTask);

        // Act
        await _sut.Handle(context);

        // Assert
        await responseHelper.Received(deleted ? 1 : 0)
                            .DeleteMessageAsync(Arg.Any<UpdateContext>());
    }
}
