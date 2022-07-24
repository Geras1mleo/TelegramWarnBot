namespace TelegramWarnBot.Tests;

public class MockedConfigurationContext : IConfigurationContext
{
    public static MockedConfigurationContext Shared { get; }

    static MockedConfigurationContext()
    {
        Shared = new();
    }

    public BotConfiguration BotConfiguration => new()
    {
        RegisteredChats = new List<long>() { 69 },
    };

    public Configuration Configuration => new Fixture().Create<Configuration>();

    public IllegalTrigger[] IllegalTriggers => new Fixture().CreateMany<IllegalTrigger>().ToArray();

    public Trigger[] Triggers => new[]
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

    public void ReloadConfiguration()
    {
        throw new NotImplementedException();
    }
}
