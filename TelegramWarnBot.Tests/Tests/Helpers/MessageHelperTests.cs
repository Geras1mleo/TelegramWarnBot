namespace TelegramWarnBot.Tests;

public class MessageHelperTests
{
    private readonly MessageHelper _sut = new();

    private readonly IFixture fixture = FixtureProvider.Fixture;

    private readonly string[] messagesToMatch = new[]
    {
        "Test",
        "Lorem ipsum",
        "ipsum Lorem",
    };

    [Theory]
    [InlineData("test", true, true, false)]
    [InlineData("test", false, true, false)]
    [InlineData("test", true, false, true)]
    [InlineData("TESTING", true, false, false)]
    [InlineData("TESTING", false, false, true)]
    [InlineData("TESTING", false, true, false)]
    [InlineData("Lorem ipsum", true, true, true)]
    [InlineData("Lorem", true, true, false)]
    [InlineData("ipsum", true, true, false)]
    public void MatchMessage_ShouldReturnTrue_WhenMatched(string message, bool matchWholeMessage, bool matchCase, bool shouldMatch)
    {
        // Act
        var matched = _sut.MatchMessage(messagesToMatch, matchWholeMessage, matchCase, message);

        // Assert
        Assert.True(matched == shouldMatch);
    }


    [Theory]
    [InlineData(MessageEntityType.BotCommand, false)]
    [InlineData(MessageEntityType.Cashtag, false)]
    [InlineData(MessageEntityType.Mention, true)]
    [InlineData(MessageEntityType.TextMention, true)]
    [InlineData(MessageEntityType.TextLink, true)]
    [InlineData(MessageEntityType.Url, true)]
    [InlineData(MessageEntityType.Spoiler, false)]
    [InlineData(MessageEntityType.PhoneNumber, false)]
    public void MatchLinkMessage_ShouldReturnTrue_WhenMatched(MessageEntityType type, bool shouldMatch)
    {
        // Arrange
        var message = fixture.BuildMessage("test", 69, 420);

        message.Entities = new MessageEntity[]
        {
            new MessageEntity()
            {
                Type = type
            }
        };

        // Act
        var matched = _sut.MatchLinkMessage(message);

        // Assert
        Assert.True(matched == shouldMatch);
    }


    [Theory]
    [InlineData("1234567891234567", true)]
    [InlineData("1234 5678 9123 4567", true)]
    [InlineData("12345678 91234567", true)]
    [InlineData("test1234567891234567hello", true)]
    [InlineData("Some message and card number 1234567891234567 end of message", true)]
    [InlineData("123456789123456 Message with 15 numbers", false)]
    [InlineData(@"Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Sed risus pretium quam vulputate dignissim suspendisse in. Sagittis id consectetur purus ut faucibus pulvinar elementum integer. Condimentum mattis pellentesque id nibh tortor. Vestibulum rhoncus est pellentesque elit ullamcorper dignissim cras tincidunt. Mauris pharetra et ultrices neque ornare aenean euismod elementum nisi. Lorem ipsum dolor sit amet consectetur adipiscing elit. Eu lobortis elementum nibh tellus molestie nunc. Cras fermentum odio eu feugiat pretium nibh ipsum. Non sodales neque sodales ut etiam sit amet nisl. Morbi tincidunt augue interdum velit euismod in pellentesque massa placerat. Sed cras ornare arcu dui vivamus. Risus quis varius quam quisque id diam vel.
                  1234567891234567 Et netus et malesuada fames ac turpis egestas sed tempus. Habitasse platea dictumst quisque sagittis purus sit amet volutpat consequat. Semper eget duis at tellus at urna condimentum. Blandit cursus risus at ultrices. Mauris nunc congue nisi vitae suscipit tellus mauris. Nec nam aliquam sem et tortor consequat id porta nibh. Volutpat est velit egestas dui id ornare. Nunc id cursus metus aliquam eleifend mi in. Turpis massa tincidunt dui ut ornare. Nunc vel risus commodo viverra maecenas accumsan lacus vel. Turpis massa tincidunt dui ut ornare lectus sit amet. Amet justo donec enim diam vulputate. Nisi est sit amet facilisis magna etiam. Porttitor leo a diam sollicitudin tempor id eu. Fringilla phasellus faucibus scelerisque eleifend. Porttitor massa id neque aliquam vestibulum morbi. Egestas fringilla phasellus faucibus scelerisque eleifend. Integer enim neque volutpat ac. Nunc id cursus metus aliquam eleifend mi. Tortor dignissim convallis aenean et.", true)]
    public void MatchCardNumber_ShouldReturnTrue_WhenMatched(string message, bool shouldMatch)
    {
        // Act
        var matched = _sut.MatchCardNumber(message);

        // Assert
        Assert.True(matched == shouldMatch);
    }
}
