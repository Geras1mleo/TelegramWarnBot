namespace TelegramWarnBot;

public interface IContext
{
    bool ResolveAttributes(Type type);
}

public class UpdateContext : IContext
{
    public Update Update { get; init; }
    public CancellationToken CancellationToken { get; init; }
    public User Bot { get; init; }
    public ChatDTO ChatDTO { get; set; }
    public UserDTO UserDTO { get; set; }
    public string Text { get; init; } // TODO refactor with references (only getters)
    public int? MessageId { get; init; }
    public bool IsText { get; init; }
    public bool IsMessageUpdate { get; init; }
    public bool IsJoinedLeftUpdate { get; init; }
    public bool IsAdminsUpdate { get; init; }
    public bool IsCommandUpdate { get; init; }
    public bool IsChatRegistered { get; init; }
    public bool IsBotAdmin { get; init; }
    public bool IsSenderAdmin { get; init; }

    public bool ResolveAttributes(Type type)
    {
        var allowed = true;

        if (type.CustomAttributes.Any(a => a.AttributeType == typeof(RegisteredChatAttribute)))
            allowed = IsChatRegistered;

        if (allowed && type.CustomAttributes.Any(a => a.AttributeType == typeof(TextMessageUpdateAttribute)))
            allowed = IsText;

        if (allowed && type.CustomAttributes.Any(a => a.AttributeType == typeof(BotAdminAttribute)))
            allowed = IsBotAdmin;

        return allowed;
    }
}