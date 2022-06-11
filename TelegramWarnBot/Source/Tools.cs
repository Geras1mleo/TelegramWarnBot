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

    public static string ResolveResponseVariables(string response, WarnedUser user, string defaultName = "Not Found")
    {
        return response.Replace("{warnedUser.WarnedCount}", user.Warnings.ToString())
                       .Replace("{warnedUser}", Tools.GetMentionString(IOHandler.GetUsers().Find(u => u.Id == user.Id)?.Name ?? defaultName, user.Id))
                       .Replace("{configuration.MaxWarnings}", (IOHandler.GetConfiguration().MaxWarnings + 1).ToString());
    }

    public static string ResolveResponseVariables(string response, UserDTO user, int warnedCount)
    {
        return response.Replace("{warnedUser.WarnedCount}", warnedCount.ToString())
                       .Replace("{warnedUser}", Tools.GetMentionString(user.Name, user.Id))
                       .Replace("{configuration.MaxWarnings}", (IOHandler.GetConfiguration().MaxWarnings + 1).ToString());
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

    public static void PrintAvailableCommands()
    {
        WriteColor(
         "\nAvailable commands:\n"

         + "\n[send] \t=> Send message:"
             + "\n\t[-c] => Chat with according chat ID. Use . to send to all chats"
             + "\n\t[-m] => Message to send. Please use \"\" to indicate message. Markdown formating allowed"
         + "\nExample: send -c 123456 -m \"Example message\"\n"

         + "\n[reload]/[r] => Reload configurations\n"
         + "\n[save]/[s] \t=> Save last data\n"
         + "\n[exit]/[e] \t=> Save data and close the application (CTRL + C)\n"
         , ConsoleColor.Red, false);
    }

    // Extensions

    public static string GetFullName(this User user)
    {
        return (user.FirstName + " " + user.LastName).Trim();
    }
}