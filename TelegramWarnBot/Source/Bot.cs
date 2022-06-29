namespace TelegramWarnBot;

public static class Bot
{
    public static BotConfiguration Configuration;
    public static User User;
    public static TelegramBotClient Client;

    public static void Start(CancellationToken cancellationToken)
    {
        Configuration = IOHandler.BotConfiguration;

        Client = new(Configuration.Token);

        Client.StartReceiving(BotHandler.HandleUpdateAsync, (_, _, _) => Task.CompletedTask,
            receiverOptions: new ReceiverOptions()
            {
                AllowedUpdates = new[] { UpdateType.Message },
            },
            cancellationToken: cancellationToken);

        User = Client.GetMeAsync(cancellationToken).GetAwaiter().GetResult();

        // Register bot itself to recognize when someone mentions it with @
        IOHandler.CacheUser(User);

        IOHandler.BeginUpdate(IOHandler.Configuration.UpdateDelay, cancellationToken);

        Tools.WriteColor($"\n\nBot: [{User.GetFullName()}] running...", ConsoleColor.Green, true);

        Tools.WriteColor($"\n[Version: {Assembly.GetEntryAssembly().GetName().Version}]", ConsoleColor.Yellow, false);

        Console.Title = User.GetFullName();

        CommandHandler.Register(new List<string>{ "-l" });
    }

    public static bool IsChatRegistered(long chatId)
    {
        return Configuration.RegisteredChats.Any(c => c == chatId);
    }
}
