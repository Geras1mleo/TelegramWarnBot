namespace TelegramWarnBot;

public interface IResponseHelper
{
    string ResolveResponseVariables(string response, UserDTO user, int warnedCount);
    string ResolveResponseVariables(string response, WarnedUser user, string defaultName = "Not Found");
}

public class ResponseHelper : IResponseHelper
{
    private readonly IConfigurationContext configurationContext;
    private readonly ICachedDataContext cachedDataContext;

    public ResponseHelper(IConfigurationContext configurationContext,
                          ICachedDataContext cachedDataContext)
    {
        this.configurationContext = configurationContext;
        this.cachedDataContext = cachedDataContext;
    }

    public string ResolveResponseVariables(string response, WarnedUser user, string defaultName = "Not Found")
    {
        return response.Replace("{warnedUser.WarnedCount}", user.Warnings.ToString())
                       .Replace("{warnedUser}", GetMentionString(cachedDataContext.Users.Find(u => u.Id == user.Id)?.Name ?? defaultName, user.Id))
                       .Replace("{configuration.MaxWarnings}", (configurationContext.Configuration.MaxWarnings).ToString());
    }

    public string ResolveResponseVariables(string response, UserDTO user, int warnedCount)
    {
        return response.Replace("{warnedUser.WarnedCount}", warnedCount.ToString())
                       .Replace("{warnedUser}", GetMentionString(user.Name, user.Id))
                       .Replace("{configuration.MaxWarnings}", (configurationContext.Configuration.MaxWarnings).ToString());
    }

    public static string GetMentionString(string caption, long id)
    {
        return $"[{caption}](tg://user?id={id})";
    }
}
