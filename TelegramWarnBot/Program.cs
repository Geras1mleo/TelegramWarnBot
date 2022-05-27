var cts = new CancellationTokenSource();

Console.InputEncoding = Console.OutputEncoding = Encoding.Unicode;
CloseHandler.Configure(cts);

try
{
    Bot.Configuration = IOHandler.GetBotConfiguration();
}
catch (Exception e)
{
    Console.WriteLine(e.Message);
    Environment.Exit(0);
}

Bot.Client = new(Bot.Configuration.Token);

Bot.Client.StartReceiving(BotHandler.HandleUpdateAsync, BotHandler.HandlePollingErrorAsync,
    receiverOptions: new ReceiverOptions() { AllowedUpdates = new[] { UpdateType.Message }, }, cancellationToken: cts.Token);

Bot.User = await Bot.Client.GetMeAsync(cts.Token);

Tools.WriteColor($"Bot: [{Bot.User.FirstName}] running...", ConsoleColor.Green);

IOHandler.BeginUpdate(IOHandler.GetConfiguration().UpdateDelay, cts.Token);

ShowInfo();

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
            if (!CommandHandler.Send(Bot.Client, parts.Skip(1).ToList(), cts.Token).GetAwaiter().GetResult())
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
            ShowInfo();
            break;
    }
}

static void ShowInfo()
{
    Tools.WriteColor(
     "\nAvailable commands:\n"

     + "\n[send] \t=> Send message:"
         + "\n\t[-c] => Chat with according chat ID. Use . to send to all chats"
         + "\n\t[-m] => Message to send. Please use \"\" to indicate message. Markdown formating allowed"
     + "\nExample: send -c 123456 -m \"Example message\"\n"

     + "\n[reload] \t=> Reload configurations\n"

     + "\n[exit] \t=> Save data and close the application (CTRL + C)\n"
     , ConsoleColor.Red);
}