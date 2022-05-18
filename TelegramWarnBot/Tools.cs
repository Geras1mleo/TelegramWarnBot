namespace TelegramWarnBot;

public static class Tools
{
    public static string GetMentionString(string caption, long id)
    {
        return $"[{caption}](tg://user?id={id})";
    }

    public static MethodInfo ResolveMethod(Type type, string prefix)
    {
        return type.GetMethods().FirstOrDefault(m => m.Name.ToLower().Equals(prefix));
    }
}
