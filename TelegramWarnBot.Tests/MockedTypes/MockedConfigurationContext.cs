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

    public Configuration Configuration => configuration;

    private Configuration configuration = new()
    {
        AllowAdminWarnings = true,
        UpdateDelay = 60,
        MaxWarnings = 3,
        DeleteJoinedLeftMessage = true,
        DeleteLinksFromNewMembers = true,
        DeleteWarnMessage = true,
        NewMemberStatusFromHours = 24,
        Captions = new()
        {
            OnBotJoinedChatMessage = "",
            ChatNotRegistered = "",
            UserNoPermissions = "",
            BotHasNoPermissions = "",
            InvalidOperation = "",
            UserNotFound = "",
            WarnedSuccessfully = "",
            UnwarnedSuccessfully = "",
            BannedSuccessfully = "",
            UnwarnUserNoWarnings = "",
            WarnAdminAttempt = "",
            UnwarnAdminAttempt = "",
            WarnBotAttempt = "",
            UnwarnBotAttempt = "",
            WarnBotSelfAttempt = "",
            UnwarnBotSelfAttempt = "",
            IllegalTriggerWarned = "",
            IllegalTriggerBanned = "",
            WCountMessage = "",
            WCountUserHasNoWarnings = "",
            WCountAdminAttempt = "",
            WCountBotAttempt = "",
            WCountBotSelfAttempt = "",
        }
    };

    public IllegalTrigger[] IllegalTriggers => illegalTriggers;

    private IllegalTrigger[] illegalTriggers = new[]
    {
        new IllegalTrigger()
        {
            DeleteMessage = true,
            IgnoreAdmins = false,
            WarnMember = true,
            IllegalWords = new[] { "word1", "word2" },
            NotifiedAdmins = new[] { 5149719899 }
        }
    };

    public Trigger[] Triggers => triggers;

    private Trigger[] triggers = new[]
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
