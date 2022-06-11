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

        // Register bot itself to recognize when someone mentions it with @
        IOHandler.RegisterUser(User.Id, User.Username, User.GetFullName());

        IOHandler.BeginUpdate(IOHandler.GetConfiguration().UpdateDelay, cancellationToken);

        Tools.WriteColor($"Bot: [{User.GetFullName()}] running...", ConsoleColor.Green, true);

        Tools.WriteColor($"\n[Version: {Assembly.GetEntryAssembly().GetName().Version}]", ConsoleColor.Yellow, false);

        Console.Title = User.GetFullName();
    }
}
