namespace TelegramWarnBot.Tests;

public class ResponseHelperTests
{
    private readonly ResponseHelper _sut;

    public ResponseHelperTests()
    {
        _sut = new ResponseHelper(Substitute.For<ITelegramBotClientProvider>(),
                                  MockedConfigurationContext.Shared,
                                  MockedCachedContext.Shared,
        new SmartFormatterProvider(Smart.CreateDefaultSmartFormat(new SmartSettings()
        {
            CaseSensitivity = CaseSensitivityType.CaseInsensitive,
        })));
    }

    [Theory]
    [InlineData("{SenderUser}", "@robert\\_johnson")]
    [InlineData("{MentionedUser}", "[Hugh Jackman](tg://user?id=510)")]
    [InlineData("{MentionedUser.Warnings} {SenderUser.Warnings}", "1 2")]
    [InlineData("Lorem ipsum dolor sit amet consectetur _italic_", "Lorem ipsum dolor sit amet consectetur _italic_")]
    [InlineData("Lorem ipsum {SenderUser} {MentionedUser} consectetur", "Lorem ipsum @robert\\_johnson [Hugh Jackman](tg://user?id=510) consectetur")]
    public void FormatResponseVariables_ShouldReturnParsedAndFormatedMessage(string template, string expected)
    {
        // Arrange
        var responseContext = new ResponseContext()
        {
            Message = template,
            MentionedUserId = 510
        };

        var updateContext = new UpdateContext()
        {
            ChatDTO = new ChatDTO
            {
                Id = 69,
            },
            UserDTO = new UserDTO()
            {
                Id = 420,
            }
        };

        // Act
        var actual = _sut.FormatResponseVariables(responseContext, updateContext);

        // Assert
        Assert.Equal(expected, actual);
    }
}
