namespace TelegramWarnBot.Tests;

public class MockedUpdateContextBuilder : IUpdateContextBuilder
{
    public static IUpdateContextBuilder Shared { get; }

    static MockedUpdateContextBuilder()
    {
        Shared = new UpdateContextBuilder(MockedCachedContext.Shared,
                                          new ChatHelper(MockedCachedContext.Shared,
                                                          MockedConfigurationContext.Shared));
    }

    public UpdateContext Build(ITelegramBotClient client, Update update, User botUser, CancellationToken cancellationToken)
    {
        return Shared.Build(client, update, botUser, cancellationToken);
    }

    public UpdateContext BuildMocked(Update update)
    {
        return Shared.Build(null, update, FixtureProvider.Fixture.BuildUser(123), CancellationToken.None);
    }
}
