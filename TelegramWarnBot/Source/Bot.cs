namespace TelegramWarnBot;

public static class Bot
{
    public static BotConfiguration Configuration;
    public static User User;
    public static TelegramBotClient Client;

    public static void Start(CancellationToken cancellationToken)
    {
        Configuration = IOHandler.GetBotConfiguration();

        Client = new(Configuration.Token);

        Client.StartReceiving(BotHandler.HandleUpdateAsync, BotHandler.HandlePollingErrorAsync,
            receiverOptions: new ReceiverOptions() { AllowedUpdates = new[] { UpdateType.Message }, }, cancellationToken: cancellationToken);

        User = Client.GetMeAsync(cancellationToken).GetAwaiter().GetResult();

        IOHandler.BeginUpdate(IOHandler.GetConfiguration().UpdateDelay, cancellationToken);

        Tools.WriteColor($"Bot: [{User.FirstName}] running...", ConsoleColor.Green);

        Tools.WriteColor($"\n[Version: {Assembly.GetEntryAssembly().GetName().Version}]", ConsoleColor.Yellow);

        Console.Title = User.FirstName;
    }
}
