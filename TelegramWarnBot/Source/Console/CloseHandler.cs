namespace TelegramWarnBot;

public interface ICloseHandler
{
    void Configure(CancellationTokenSource cancellationTokenSource);
}

public class CloseHandler : ICloseHandler
{
    private CancellationTokenSource cancellationTokenSource;
    private readonly ICachedDataContext cachedDataContext;

    public CloseHandler(ICachedDataContext cachedDataContext)
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