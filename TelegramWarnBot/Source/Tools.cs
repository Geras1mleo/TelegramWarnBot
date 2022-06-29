namespace TelegramWarnBot;

public static class Tools
{
    public static string GetMentionString(string caption, long id)
    {
        return $"[{caption}](tg://user?id={id})";
    }

    private static readonly Dictionary<Type, MethodInfo[]> methodsDict = new();
    public static MethodInfo ResolveMethod(Type type, string prefix)
    {
        if (methodsDict.TryGetValue(type, out var cachedMethods))
            return cachedMethods.FirstOrDefault(m => m.Name.ToLower().Equals(prefix));

        var methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
        methodsDict.Add(type, methods);
        return methods.FirstOrDefault(m => m.Name.ToLower().Equals(prefix));
    }

    public static string ResolveResponseVariables(string response, WarnedUser user, string defaultName = "Not Found")
    {
        return response.Replace("{warnedUser.WarnedCount}", user.Warnings.ToString())
                       .Replace("{warnedUser}", Tools.GetMentionString(IOHandler.Users
                                                                                    .Find(u => u.Id == user.Id)?
                                                                                    .Name ?? defaultName,
                                                                                user.Id))
                       .Replace("{configuration.MaxWarnings}", (IOHandler.Configuration.MaxWarnings).ToString());
    }

    public static string ResolveResponseVariables(string response, UserDTO user, int warnedCount)
    {
        return response.Replace("{warnedUser.WarnedCount}", warnedCount.ToString())
                       .Replace("{warnedUser}", Tools.GetMentionString(user.Name, user.Id))
                       .Replace("{configuration.MaxWarnings}", (IOHandler.Configuration.MaxWarnings).ToString());
    }

    // https://stackoverflow.com/questions/2743260/is-it-possible-to-write-to-the-console-in-colour-in-net
    // usage: WriteColor("This is my [message] with inline [color] changes.", ConsoleColor.Yellow);
    public static void WriteColor(string message, ConsoleColor color, bool logDateTime)
    {
        if (logDateTime)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write($"[{DateTime.Now}] ");
            Console.ResetColor();
        }

        var pieces = Regex.Split(message, @"(\[[^\]]*\])");

        for (int i = 0; i < pieces.Length; i++)
        {
            string piece = pieces[i];

            if (piece.StartsWith("[") && piece.EndsWith("]"))
            {
                Console.ForegroundColor = color;
                piece = piece[1..^1];
            }

            Console.Write(piece);
            Console.ResetColor();
        }

        Console.WriteLine();
    }

    // Extensions

    public static string GetFullName(this User user)
    {
        return (user.FirstName + " " + user.LastName).Trim();
    }
}