
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

client.StartReceiving(BotHandlers.HandleUpdateAsync, BotHandlers.HandlePollingErrorAsync,
    receiverOptions: new ReceiverOptions() { AllowedUpdates = new[] { UpdateType.Message }, }, cancellationToken: cts.Token);

BotHandlers.MeUser = await client.GetMeAsync(cts.Token);

Console.WriteLine($"Bot {BotHandlers.MeUser.FirstName} running...");

ShowInfo();

IOHandler.BeginUpdateAsync(60, cts.Token); // todo from config

CloseHandler.Configure();

while (true)
{
    var command = Console.ReadLine();
    var params = command.Split(' ');

    switch (command)
    {
        case "send":
            if(!CommandHandler.Send(client, params[1..])) // Returned false => not succeed => show commands 
                goto default: 
            break;
        case "exit":
            CommandHandler.Exit(cts);
            break;
        default:
            Console.WriteLine("Not recognized...");
            ShowInfo();
            break;
    }
}

void ShowInfo()
{
    Console.WriteLine("Available commands:"
                + "\n\tsend => Send message to:"
                    + "\n\t\t-c => Chat with according chat ID"
                    + "\n\t\t-u => User with according user ID (private message)"
                    + "\n\t\t-m => Mention user in message: caption/userid"
                + "\n\texit - Save data and close application... (CTRL + C)"
    );    
}