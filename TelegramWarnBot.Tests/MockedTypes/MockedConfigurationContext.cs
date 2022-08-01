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
        Token = "token"
    };

    public Configuration Configuration
    {
        get
        {
            return new()
            {
                AllowAdminWarnings = true,
                UpdateDelay = 60,
                MaxWarnings = 3,
                DeleteJoinedLeftMessage = true,
                DeleteLinksFromNewMembers = true,
                DeleteWarnMessage = true,
                NewMemberStatusFromHours = 24,
                Captions = new Fixture().Create<Captions>()
            };
        }
    }

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
