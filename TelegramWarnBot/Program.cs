
//TelegramBotClient ca = new("5149219899:AAEBeAGygk97tRrHxr3MpTZwo-bo9BgYHkM"); // testing bot
var cts = new CancellationTokenSource();
Configuration config = null;

Console.InputEncoding = Console.OutputEncoding = Encoding.Unicode;
CloseHandler.Configure(cts);

try
{
    config = IOHandler.GetConfiguration();
}
catch (Exception e)
{
    Console.WriteLine(e.Message);
    Environment.Exit(0);
}

TelegramBotClient client = new(config.Token);

client.StartReceiving(BotHandler.HandleUpdateAsync, BotHandler.HandlePollingErrorAsync,
    receiverOptions: new ReceiverOptions() { AllowedUpdates = new[] { UpdateType.Message }, }, cancellationToken: cts.Token);

BotHandler.MeUser = await client.GetMeAsync(cts.Token);

Console.WriteLine($"Bot {BotHandler.MeUser.FirstName} running...");

ShowInfo();

IOHandler.BeginUpdate(config.UpdateDelay, cts.Token);

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
            if (!CommandHandler.Send(client, parts.Skip(1).ToList(), cts.Token).GetAwaiter().GetResult())
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