namespace TelegramWarnBot;

public static class CloseHandler
{
    public static CancellationTokenSource CancellationTokenSource;

    public static void Configure(CancellationTokenSource cancellationTokenSource)
    {
        Console.CancelKeyPress += delegate
        {
            Environment.Exit(1);
        };

        AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;
        CancellationTokenSource = cancellationTokenSource;
    }

    private static void CurrentDomain_ProcessExit(object sender, EventArgs e)
    {
        CancellationTokenSource.Cancel();
        Console.WriteLine("Saving data...");
        IOHandler.SaveData();
    }
}