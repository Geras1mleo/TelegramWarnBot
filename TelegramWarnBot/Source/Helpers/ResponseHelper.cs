namespace TelegramWarnBot;

public class ResponseHelper
{
    private readonly ConfigurationContext configurationContext;
    private readonly CachedDataContext cachedDataContext;

    public ResponseHelper(ConfigurationContext configurationContext,
                          CachedDataContext cachedDataContext)
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
