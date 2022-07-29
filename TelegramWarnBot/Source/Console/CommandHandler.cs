namespace TelegramWarnBot;

public interface IConsoleCommandHandler
{
    void Start(CancellationToken cancellationToken);
}

public class ConsoleCommandHandler : IConsoleCommandHandler
{
    private readonly IServiceProvider serviceProvider;
    private readonly ILogger<ConsoleCommandHandler> logger;

    public ConsoleCommandHandler(IServiceProvider serviceProvider,
                                 ILogger<ConsoleCommandHandler> logger)
    {
        this.serviceProvider = serviceProvider;
        this.logger = logger;
    }

    public void Start(CancellationToken cancellationToken)
    {
        Task.Run(() =>
        {
            string commandInput;
            string[] commandArgs;

            while (!cancellationToken.IsCancellationRequested)
            {
                // Creating every time new object bc of some bugs...
                var console = new CommandLineApplicationWithDI(serviceProvider)
                {
                    UnrecognizedArgumentHandling = UnrecognizedArgumentHandling.CollectAndContinue,
                };
                commandInput = Console.ReadLine();

                if (commandInput is null) continue;

                commandArgs = Tools.ConsoleCommandRegex.Matches(commandInput)
                                                       .Cast<Match>()
                                                       .Select(m => m.Value)
                                                       .ToArray();

                if (commandArgs.Length > 0)
                {
                    commandArgs[0] = commandArgs[0] switch
                    {
                        "l" => "leave",
                        "r" => "reload",
                        "s" => "save",
                        "i" => "info",
                        "v" => "version",
                        _ => commandArgs[0]
                    };
                }

                try
                {
                    console.Execute(commandArgs);
                }
                catch (Exception e)
                {
                    logger.LogWarning(e, "Error while executing command");
                }
            }
        }, cancellationToken);
    }
}