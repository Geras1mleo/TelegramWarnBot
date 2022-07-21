
// A wise man once said:"If your code is hard to test, u wrote bad code, my friend..."
// Soo.. my code is just awful

namespace TelegramWarnBot.Tests;

public class UpdateHandlersTests
{
    private readonly Mock<ITelegramBotClient> mockClient = new();
    private readonly Mock<IConfigurationContext> mockConfigurationContext = new();
    private readonly Mock<ICachedDataContext> mockCachedDataContext = new();
    private readonly Mock<IChatHelper> mockChatHelper = new();
    private readonly MessageHelper messageHelper = new();
    private readonly Mock<IResponseHelper> mockResponseHelper = new();

    UpdateContext Context { get; } = new UpdateContext()
    {
        Update = new Update() { },
        Bot = new User()
        {
            Id = Constants.BotId,
        },
        ChatDTO = new ChatDTO()
        {
            Id = Constants.ChatId,
            Admins = new List<long>()
            {
                Constants.Admin1,
                Constants.Admin2,
                Constants.Admin3,
            }
        },
        CancellationToken = CancellationToken.None,
    };

    Trigger[] Triggers { get; } = new Trigger[]
    {
        new Trigger()
        {
            Messages = new string[]{ "good russians" },
            Responses = new string[]{ "Do not exist!" },
        },
        new Trigger()
        {
            Messages = new string[]{ "pidor"},
            Responses = new string[]{ "{SenderUser} is gay" },
            MatchCase  = true,
            MatchWholeMessage = true
        }
    };

    [Theory]
    [InlineData("good russians", "Do not exist!")]
    [InlineData("GOOD russIANS", "Do not exist!")]
    [InlineData("bad russians", null)]
    [InlineData("just a message", null)]
    [InlineData("just a message with good russians", "Do not exist!")]
    [InlineData("pidor", "{SenderUser} is gay")]
    [InlineData("pidor writing", null)]
    [InlineData("Pidor", null)]
    public async Task TestTriggersHandler(string message, string response = null)
    {
        var handler = new TriggersHandler(c => Task.CompletedTask,
                                          mockConfigurationContext.Object,
                                          messageHelper,
                                          mockResponseHelper.Object);

        Context.Update.Message = new Message()
        {
            MessageId = 69,
            Text = message,
            From = Constants.Sender,
        };

        mockConfigurationContext.SetupGet(x => x.Triggers).Returns(Triggers);

        if (response is not null)
        {
            mockResponseHelper.Setup(x => x.SendMessageAsync
                                    (It.Is<ResponseContext>(c => c.Message == response),
                                     Context,
                                     Context.Update.Message.MessageId))
                              .Returns(Task.CompletedTask);
        }

        await handler.Handle(Context);

        Mock.VerifyAll(mockConfigurationContext, mockResponseHelper);
    }
}
