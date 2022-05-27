namespace TelegramWarnBot;

public static class CloseHandler
{
    private static CancellationTokenSource cancellationTokenSource;

    public static void Configure(CancellationTokenSource cancellationTokenS)
    {
        Console.CancelKeyPress += delegate
        {
            Environment.Exit(1);
        };

        AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;
        cancellationTokenSource = cancellationTokenS;
    }

    private static void CurrentDomain_ProcessExit(object sender, EventArgs e)
    {
        cancellationTokenSource.Cancel();
        Console.WriteLine("Saving data...");
        IOHandler.SaveData();
    }
}