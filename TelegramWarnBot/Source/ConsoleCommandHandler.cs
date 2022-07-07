using static TelegramWarnBot.Tools;

namespace TelegramWarnBot;

public static class CommandHandler
{
    public static void StartListening(CancellationToken cancellationToken)
    {
        //PrintAvailableCommands();

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

                case "register":
                    if (!Register(parts.Skip(1).ToList()))
                        goto default;
                    break;

                case "reload":
                    IOHandler.ReloadConfiguration();
                    Tools.WriteColor("[Configuration reloaded successfully!]", ConsoleColor.Green, true);
                    break;

                case "leave":
                    if (long.TryParse(parts[1], out var chatId))
                        try
                        {
                            Bot.Client.LeaveChatAsync(chatId, cancellationToken: cancellationToken).GetAwaiter().GetResult();
                        }
                        catch (Exception e)
                        {
                            Tools.WriteColor("[Error]: " + e.Message, ConsoleColor.Red, false);
                        }
                    break;

                case "save": IOHandler.SaveData(); break;
                case "info": WriteInfo(); break;
                case "exit": Environment.Exit(1); break;

                case "l": goto case "leave";
                case "r": goto case "reload";
                case "s": goto case "save";
                case "e": goto case "exit";
                case "i": goto case "info";

                default:
                    WriteColor("Not recognized...", ConsoleColor.Gray, false);
                    PrintAvailableCommands();
                    break;
            }
        }
    }

    public static bool Register(List<string> parameters)
    {
        if (parameters.Count == 1)
        {
            if (long.TryParse(parameters[0], out var newChatId))
            {
                Bot.Configuration.RegisteredChats.Add(newChatId);
                IOHandler.SaveRegisteredChatsAsync().GetAwaiter().GetResult();
                WriteColor("[Chat registered successfully]", ConsoleColor.Green, true);
                return true;
            }

            if (parameters[0] == "-l")
            {
                Console.WriteLine("\nRegistered chats:");
                foreach (var chatId in Bot.Configuration.RegisteredChats)
                {
                    WriteColor("\t[" + (IOHandler.Chats.Find(c => c.Id == chatId)?.Name ?? "Chat not cached yet") + "]: " + chatId,
                                     ConsoleColor.Blue, false);
                }

                var notRegistered = IOHandler.Chats.Where(cached => !Bot.Configuration.RegisteredChats.Contains(cached.Id));
                if (!notRegistered.Any())
                    return true;

                Console.WriteLine("\nNot registered chats:");
                foreach (var chat in notRegistered)
                {
                    WriteColor("\t[" + chat.Name + "]: " + chat.Id, ConsoleColor.Red, false);
                }

                return true;
            }
        }
        else if (parameters.Count == 2)
        {
            if (parameters[0] == "-rm" && long.TryParse(parameters[1], out var removedChatId))
            {
                if (Bot.Configuration.RegisteredChats.Remove(removedChatId))
                {
                    IOHandler.SaveRegisteredChatsAsync();
                    WriteColor("[Chat removed successfully]", ConsoleColor.Green, true);
                }
                else
                    WriteColor("[Chat not found...]", ConsoleColor.Red, true);

                return true;
            }
        }

        return false;
    }

    public static Task<bool> Send(TelegramBotClient client, List<string> parameters, CancellationToken cancellationToken)
    {
        int chatIndex = parameters.FindIndex(p => p.ToLower() == "-c");

        if (chatIndex < 0)
            return Task.FromResult(false);

        string chatParameter = parameters.ElementAtOrDefault(chatIndex + 1);

        bool broadcast = chatParameter == ".";

        long chatId = 0;

        if (!broadcast && !long.TryParse(chatParameter, out chatId))
            return Task.FromResult(false);

        int messageIndex = parameters.FindIndex(p => p.ToLower() == "-m");

        if (messageIndex < 0)
            return Task.FromResult(false);

        string message = parameters.ElementAtOrDefault(messageIndex + 1);

        if (message is null || !message.StartsWith("\"") || !message.EndsWith("\"") || message.Length == 1)
            return Task.FromResult(false);

        message = message[1..^1];

        var chats = new List<ChatDTO>();

        if (broadcast)
        {
            chats = IOHandler.Chats;
        }
        else
        {
            chats.Add(new()
            {
                Id = chatId
            });
        }

        var tasks = new List<Task>();

        int sentCount = 0;
        for (int i = 0; i < chats.Count; i++)
        {
            try
            {
                if (chats[i].Id != 0)
                {
                    tasks.Add(client.SendTextMessageAsync(chats[i].Id, message, cancellationToken: cancellationToken, parseMode: ParseMode.Markdown));
                    sentCount++;
                }
            }
            catch (Exception) { }
        }

        WriteColor($"[Messages sent: {sentCount}]", ConsoleColor.Yellow, true);

        Task.WhenAll(tasks).GetAwaiter().GetResult();

        return Task.FromResult(true);
    }

    private static void WriteInfo()
    {
        if (IOHandler.Chats.Count > 0)
        {
            WriteColor($"\nCached Chats: [{IOHandler.Chats.Count}]", ConsoleColor.DarkYellow, true);

            string userName;

            foreach (var chat in IOHandler.Chats)
            {
                WriteColor($"\t[{chat.Name}]", ConsoleColor.DarkMagenta, false);

                WriteColor($"\tAdmins: [{chat.Admins.Length}]", ConsoleColor.DarkYellow, false);

                foreach (var admin in chat.Admins)
                {
                    userName = IOHandler.Users.Find(u => u.Id == admin)?.Name ?? $"Not found - {admin}";

                    WriteColor($"\t\t[{userName}]", ConsoleColor.DarkMagenta, false);
                }
                Console.WriteLine();
            }
        }

        WriteColor($"\nCached Users: [{IOHandler.Users.Count}]", ConsoleColor.DarkYellow, false);

        //if (IOHandler.Users.Count > 0)
        //{
        //    foreach (var user in IOHandler.Users)
        //    {
        //        WriteColor($"\t[{user.Name}]", ConsoleColor.DarkMagenta, false);
        //    }
        //}

        if (IOHandler.Warnings.Count > 0)
        {
            WriteColor($"\nWarnings: [{IOHandler.Warnings.SelectMany(w => w.WarnedUsers).Select(u => u.Warnings).Sum()}]", ConsoleColor.DarkYellow, false);

            string chatName, userName;

            foreach (var warning in IOHandler.Warnings)
            {
                chatName = IOHandler.Chats.Find(c => c.Id == warning.ChatId)?.Name ?? $"Not found - {warning.ChatId}";

                WriteColor($"\t[{chatName}]:", ConsoleColor.DarkMagenta, false);

                foreach (var user in warning.WarnedUsers)
                {
                    userName = IOHandler.Users.Find(u => u.Id == user.Id)?.Name ?? $"Not found - {user.Id}";

                    WriteColor($"\t\t[{userName}] - [{user.Warnings}]", ConsoleColor.DarkMagenta, false);
                }
                Console.WriteLine();
            }
        }
    }

    public static void PrintAvailableCommands()
    {
        WriteColor(
         "\nAvailable commands:\n"

         + "\n[send] \t=> Send message:"
             + "\n\t[-c] => Chat with according chat ID. Use . to send to all chats"
             + "\n\t[-m] => Message to send. Please use \"\" to indicate message. Markdown formatting allowed"
         + "\nExample: send -c 123456 -m \"Example message\"\n"

         + "\n[register] => Register new chat:"
             + "\n\t[-l] => List of registered chats"
             + "\n\t[-rm] => Remove one specific chat\n"

         + "\n[leave]/[l] => Leave a chat"
         + "\n[reload]/[r] => Reload configurations\n"
         + "\n[save]/[s] \t=> Save last data\n"
         + "\n[info]/[i] \t=> Show info about cached chats and users\n"
         + "\n[exit]/[e] \t=> Save data and close the application (CTRL + C)\n"
         , ConsoleColor.Red, false);
    }
}