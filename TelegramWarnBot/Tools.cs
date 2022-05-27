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

    // https://stackoverflow.com/questions/2743260/is-it-possible-to-write-to-the-console-in-colour-in-net
    // usage: WriteColor("This is my [message] with inline [color] changes.", ConsoleColor.Yellow);
    public static void WriteColor(string message, ConsoleColor color)
    {
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
}
