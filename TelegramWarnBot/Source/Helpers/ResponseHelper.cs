namespace TelegramWarnBot;

public interface IResponseHelper
{
    string ResolveResponseVariables(UpdateContext context, string response, long? mentionedUserId);
}

public class ResponseHelper : IResponseHelper
{
    private readonly IConfigurationContext configurationContext;
    private readonly ICachedDataContext cachedDataContext;
    private readonly SmartFormatter formatter;

    public ResponseHelper(IConfigurationContext configurationContext,
                          ICachedDataContext cachedDataContext,
                          SmartFormatter formatter)
    {
        this.configurationContext = configurationContext;
        this.cachedDataContext = cachedDataContext;
        this.formatter = formatter;
    }

    public string ResolveResponseVariables(UpdateContext context, string response, long? mentionedUserId)
    {
        var obj = new
        {
            SenderUser = GetUserObject(context, context.Update.Message.From.Id),
            MentionedUser = GetUserObject(context, mentionedUserId),
            configurationContext.Configuration
        };

        var a = formatter.Format(response, obj);

        return a;
    }

    public MentionedUserDTO GetUserObject(UpdateContext context, long? userId)
    {
        if (userId is null)
            return null;

        UserDTO user = cachedDataContext.Users.Find(u => u.Id == userId);

        if (user is null)
            return null;

        WarnedUser warnedUser = cachedDataContext.Warnings.Find(c => c.ChatId == context.ChatDTO.Id)?
                                                 .WarnedUsers.Find(u => u.Id == userId);

        return new()
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Username = user.Username,
            Warnings = warnedUser?.Warnings
        };
    }
}
