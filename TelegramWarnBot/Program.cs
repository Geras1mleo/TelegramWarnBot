
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

    var parameters = command.Split(' ');

    switch (command)
    {
        case "send":
            if (!CommandHandler.Send(client, parameters[1..])) // Returned false => not succeed => show commands 
                goto default;
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
        + "\n\t-c => Chat with according chat ID"
        + "\n\t-u => User with according user ID (private message)"
        + "\n\t-m => Mention user in message: user_name/user_id"
    + "\nexit => Save data and close the application (CTRL + C)"

    + "\n"
    );
}