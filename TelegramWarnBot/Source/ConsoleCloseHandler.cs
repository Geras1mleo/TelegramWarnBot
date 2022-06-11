namespace TelegramWarnBot;

public static class CloseHandler
{
    private static CancellationTokenSource cancellationTokenSource;

    public static void Configure(CancellationTokenSource cancellationTokenSource)
    {
        Console.CancelKeyPress += delegate
        {
            Environment.Exit(1);
        };

        AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;
        CloseHandler.cancellationTokenSource = cancellationTokenSource;
    }

    private static void CurrentDomain_ProcessExit(object sender, EventArgs e)
    {
        cancellationTokenSource.Cancel();
        Tools.WriteColor("Saving data...", ConsoleColor.Yellow, true);
        IOHandler.SaveData();
    }
}