namespace TelegramWarnBot;

public static class CommandHandler
{
    public static void StartListening(CancellationToken cancellationToken)
    {
        Tools.PrintAvailableCommands();

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
                    Console.WriteLine("Configuration reloaded...");
                    break;
                case "exit":
                    Environment.Exit(1);
                    break;
                default:
                    Console.WriteLine("Not recognized...");
                    Tools.PrintAvailableCommands();
                    break;
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
            chats = IOHandler.GetWarnings().Chats;
        else
        {
            var chat = IOHandler.GetWarnings().Chats.FirstOrDefault(c => c.Id == chatId);

            if (chat is null)
            {
                Console.WriteLine("Chat not found...");
                return true;
            }

            chats.Add(chat);
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

        Console.WriteLine("Messages sent: " + sentCount);

        return true;
    }
}