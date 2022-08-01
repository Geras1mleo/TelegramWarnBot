namespace TelegramWarnBot;

// Interface is needed to filter handlers by attributes on update
public interface IContext
{
    bool ResolveAttributes(Type type) => true;
}

public class UpdateContext : IContext
{
    public Update Update { get; set; }
    public CancellationToken CancellationToken { get; set; }
    public User Bot { get; set; }
    public ChatDTO ChatDTO { get; set; }
    public UserDTO UserDTO { get; set; }
    public bool IsText { get; set; }
    public bool IsMessageUpdate { get; set; }
    public bool IsJoinedLeftUpdate { get; set; }
    public bool IsAdminsUpdate { get; set; }
    public bool IsCommandUpdate { get; set; }
    public bool IsChatRegistered { get; set; }
    public bool IsBotAdmin { get; set; }
    public bool IsSenderAdmin { get; set; }

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
