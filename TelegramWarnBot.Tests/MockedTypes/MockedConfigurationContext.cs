namespace TelegramWarnBot.Tests;

public class MockedConfigurationContext : IConfigurationContext
{
    public BotConfiguration BotConfiguration => new()
    {
        RegisteredChats = new List<long>() { 69 },
        Token = "token"
    };

    public Configuration Configuration => configuration;

    private readonly Configuration configuration = new()
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
            OnBotJoinedChatMessage = "OnBotJoinedChatMessage",
            ChatNotRegistered = "ChatNotRegistered",
            UserNoPermissions = "UserNoPermissions",
            BotHasNoPermissions = "BotHasNoPermissions",
            InvalidOperation = "InvalidOperation",
            UserNotFound = "UserNotFound",
            WarnedSuccessfully = "WarnedSuccessfully",
            UnwarnedSuccessfully = "UnwarnedSuccessfully",
            BannedSuccessfully = "BannedSuccessfully",
            UnwarnUserNoWarnings = "UnwarnUserNoWarnings",
            WarnAdminAttempt = "WarnAdminAttempt",
            UnwarnAdminAttempt = "UnwarnAdminAttempt",
            WarnBotAttempt = "WarnBotAttempt",
            UnwarnBotAttempt = "UnwarnBotAttempt",
            WarnBotSelfAttempt = "WarnBotSelfAttempt",
            UnwarnBotSelfAttempt = "UnwarnBotSelfAttempt",
            IllegalTriggerWarned = "IllegalTriggerWarned",
            IllegalTriggerBanned = "IllegalTriggerBanned",
            WCountMessage = "WCountMessage",
            WCountUserHasNoWarnings = "WCountUserHasNoWarnings",
            WCountAdminAttempt = "WCountAdminAttempt",
            WCountBotAttempt = "WCountBotAttempt",
            WCountBotSelfAttempt = "WCountBotSelfAttempt",
        }
    };

    public IllegalTrigger[] IllegalTriggers => illegalTriggers;

    private readonly IllegalTrigger[] illegalTriggers = new[]
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

    private readonly Trigger[] triggers = new[]
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
    { }
}
