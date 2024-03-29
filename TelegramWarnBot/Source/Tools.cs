﻿namespace TelegramWarnBot;

public static class Tools
{
    public static readonly Regex CardNumberRegex = new(@"\d{4} ?\d{4} ?\d{4} ?\d{4}", RegexOptions.Compiled, TimeSpan.FromMilliseconds(250));

    public static readonly Regex ConsoleCommandRegex = new(@"[\""].+?[\""]|[^ ]+", RegexOptions.Compiled, TimeSpan.FromMilliseconds(250));

    // Cached methods
    private static readonly Dictionary<Type, MethodInfo[]> methodsDict = new();

    public static MethodInfo ResolveMethod(Type type, string name)
    {
        if (!methodsDict.TryGetValue(type, out var cachedMethods))
        {
            cachedMethods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
            methodsDict.Add(type, cachedMethods);
        }

        return cachedMethods.FirstOrDefault(m => m.Name.ToLower().Equals(name));
    }

    /// <summary>
    ///     Usage: WriteColor("This is my [message] with inline [color] changes.", ConsoleColor.Yellow);
    ///     https://stackoverflow.com/questions/2743260/is-it-possible-to-write-to-the-console-in-colour-in-net
    /// </summary>
    /// <param name="message"></param>
    /// <param name="color"></param>
    /// <param name="logDateTime"></param>
    public static void WriteColor(string message, ConsoleColor color, bool logDateTime)
    {
        if (logDateTime)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write($"[{DateTime.Now}] ");
            Console.ResetColor();
        }

        var pieces = Regex.Split(message, @"(\[[^\]]*\])");

        for (var i = 0; i < pieces.Length; i++)
        {
            var piece = pieces[i];

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