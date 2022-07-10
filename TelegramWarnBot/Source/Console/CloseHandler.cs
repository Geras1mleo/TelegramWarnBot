namespace TelegramWarnBot;

public class CloseHandler
{
    private CancellationTokenSource cancellationTokenSource;
    private readonly CachedDataContext cachedDataContext;

    public CloseHandler(CachedDataContext cachedDataContext)
    {
        this.cachedDataContext = cachedDataContext;
    }

    public void Configure(CancellationTokenSource cancellationTokenSource)
    {
        this.cancellationTokenSource = cancellationTokenSource;

        Console.CancelKeyPress += delegate
        {
            Environment.Exit(1);
        };

        AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;
    }

    private void CurrentDomain_ProcessExit(object sender, EventArgs e)
    {
        cancellationTokenSource.Cancel();
        Tools.WriteColor("Saving data...", ConsoleColor.Yellow, true);
        cachedDataContext.SaveData();
    }
}