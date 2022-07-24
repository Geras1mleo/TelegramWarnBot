namespace TelegramWarnBot.Tests;

public class FixtureProvider
{
    public static IFixture Fixture { get; }

    static FixtureProvider()
    {
        Fixture = new Fixture();

        Fixture.Behaviors.Remove(new ThrowingRecursionBehavior());
        Fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        Fixture.Customizations.Add(new TypeRelay(
                                   typeof(ChatMember),
                                   typeof(ChatMemberMember)));
    }
}
