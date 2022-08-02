namespace TelegramWarnBot.Tests;

public class MockedUpdateContextBuilder : IUpdateContextBuilder
{
    public static IUpdateContextBuilder Shared { get; }

    static MockedUpdateContextBuilder()
    {
        var chatHelper = Substitute.For<IChatHelper>();

        chatHelper.IsChatRegistered(Arg.Any<long>())
            .Returns(c => new MockedConfigurationContext().BotConfiguration.RegisteredChats.
                    Any(chat => chat == c.Arg<long>()));

        Shared = new UpdateContextBuilder(new MockedCachedContext(),
                                          chatHelper);
    }

    public UpdateContext BuildMocked(Update update)
    {
        return Shared.Build(update, FixtureProvider.Fixture.BuildUser(123), CancellationToken.None);
    }

    public UpdateContext Build(Update update, User botUser, CancellationToken cancellationToken)
    {
        return Shared.Build(update, botUser, cancellationToken);
    }
}
