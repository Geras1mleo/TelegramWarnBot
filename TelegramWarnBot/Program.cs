
//TelegramBotClient ca = new("5149219899:AAEBeAGygk97tRrHxr3MpTZwo-bo9BgYHkM"); // testing bot

var cts = new CancellationTokenSource();
Configuration config = null;

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

CloseHandler.Configure(cts);

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
            if (!CommandHandler.Send(client, parts.Skip(1).ToList(), cts.Token)) 
                goto default; // if not succeed => show available commands 
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
    Console.WriteLine(
    "\nAvailable commands:\n"
    
    + "\nsend => Send message to:"
        + "\n\t-c => Chat with according chat ID. Use . to send to all chats"
        + "\n\t-m => Message to send. Please use \"\" to indicate message. Markdown formating allowed\n"
    
    + "\nexit => Save data and close the application (CTRL + C)\n"

    );
}