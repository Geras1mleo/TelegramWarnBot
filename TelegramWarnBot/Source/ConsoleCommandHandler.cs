using static TelegramWarnBot.Tools;

namespace TelegramWarnBot;

public static class CommandHandler
{
    public static void StartListening(CancellationToken cancellationToken)
    {
        PrintAvailableCommands();

        while (true)
        {
            var command = Console.ReadLine();

            if (command is null) continue;

            var parts = Regex.Matches(command, @"[\""].+?[\""]|[^ ]+")
                             .Cast<Match>()
                             .Select(m => m.Value)
                             .ToArray();

            if (parts.Length == 0)
                continue;

            switch (parts[0])
            {
                case "send":
                    if (!Send(Bot.Client, parts.Skip(1).ToList(), cancellationToken).GetAwaiter().GetResult())
                        goto default; // if not succeed => show available commands 
                    break;
                case "reload":
                    IOHandler.ReloadConfiguration();
                    Tools.WriteColor("[Configuration reloaded successfully!]", ConsoleColor.Green, true);
                    break;
                case "save":
                    IOHandler.SaveData();
                    break;
                case "info":
                    WriteInfo();
                    break;
                case "exit":
                    Environment.Exit(1);
                    break;

                case "s": goto case "save";
                case "e": goto case "exit";
                case "r": goto case "reload";
                case "i": goto case "info";

                default:
                    WriteColor("Not recognized...", ConsoleColor.Gray, false);
                    PrintAvailableCommands();
                    break;
            }
        }
    }

    private static void WriteInfo()
    {
        if (IOHandler.GetChats().Count > 0)
        {
            WriteColor($"\nRegistered Chats: [{IOHandler.GetChats().Count}]", ConsoleColor.DarkMagenta, true);

            foreach (var chat in IOHandler.GetChats())
            {
                WriteColor($"\t[{chat.Name}]", ConsoleColor.DarkMagenta, false);
            }
        }

        if (IOHandler.GetUsers().Count > 0)
        {
            WriteColor($"\nRegistered Users: [{IOHandler.GetUsers().Count}]", ConsoleColor.DarkMagenta, false);

            foreach (var user in IOHandler.GetUsers())
            {
                WriteColor($"\t[{user.Name}]", ConsoleColor.DarkMagenta, false);
            }
        }

        if (IOHandler.GetWarnings().Count > 0)
        {
            Console.WriteLine("\nWarnings:");

            string chatName, userName;

            foreach (var warning in IOHandler.GetWarnings())
            {
                chatName = IOHandler.GetChats().Find(c => c.Id == warning.ChatId)?.Name ?? "Not found...";

                WriteColor($"\t[{chatName}]:", ConsoleColor.DarkMagenta, false);

                foreach (var user in warning.WarnedUsers)
                {
                    userName = IOHandler.GetUsers().Find(u => u.Id == user.Id)?.Name ?? "Not found...";
                    WriteColor($"\t\t[{userName}] - [{user.Warnings}]", ConsoleColor.DarkMagenta, false);
                }
            }
        }
    }

    public static async Task<bool> Send(TelegramBotClient client, List<string> parameters, CancellationToken cancellationToken)
    {
        int chatIndex = parameters.FindIndex(p => p.ToLower() == "-c");

        if (chatIndex < 0)
            return false;

        string chatParameter = parameters.ElementAtOrDefault(chatIndex + 1);

        bool broadcast = chatParameter == ".";

        long chatId = 0;

        if (!broadcast && !long.TryParse(chatParameter, out chatId))
            return false;

        int messageIndex = parameters.FindIndex(p => p.ToLower() == "-m");

        if (messageIndex < 0)
            return false;

        string message = parameters.ElementAtOrDefault(messageIndex + 1);

        if (message is null || !message.StartsWith("\"") || !message.EndsWith("\"") || message.Length == 1)
            return false;

        message = message[1..^1];

        var chats = new List<ChatDTO>();

        if (broadcast)
        {
            chats = IOHandler.GetChats();
        }
        else
        {
            chats.Add(new()
            {
                Id = chatId
            });
        }

        int sentCount = 0;
        for (int i = 0; i < chats.Count; i++)
        {
            try
            {
                if (chats[i].Id != 0)
                {
                    await client.SendTextMessageAsync(chats[i].Id, message, cancellationToken: cancellationToken, parseMode: ParseMode.Markdown);
                    sentCount++;
                }
            }
            catch (Exception) { }
        }

        WriteColor($"[Messages sent: {sentCount}]", ConsoleColor.Yellow, true);

        return true;
    }

    public static void PrintAvailableCommands()
    {
        WriteColor(
         "\nAvailable commands:\n"

         + "\n[send] \t=> Send message:"
             + "\n\t[-c] => Chat with according chat ID. Use . to send to all chats"
             + "\n\t[-m] => Message to send. Please use \"\" to indicate message. Markdown formatting allowed"
         + "\nExample: send -c 123456 -m \"Example message\"\n"

         + "\n[reload]/[r] => Reload configurations\n"
         + "\n[save]/[s] \t=> Save last data\n"
         + "\n[info]/[i] \t=> Show info about registered chats and users\n"
         + "\n[exit]/[e] \t=> Save data and close the application (CTRL + C)\n"
         , ConsoleColor.Red, false);
    }
}