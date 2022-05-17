
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

while (true)
{
    var command = Console.ReadLine();

    switch (command)
    {
        case "send":
            // todo
            break;
        case "exit":
            goto ExitLabel;
        default:
            Console.WriteLine("Not Recognized...");
            break;
    }
}

ExitLabel:

await IOHandler.SaveUsersAsync();
await IOHandler.SaveWarningsAsync();

cts.Cancel();