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
            var console = new CommandLineApplicationWithDI(serviceProvider)
            {
                UnrecognizedArgumentHandling = UnrecognizedArgumentHandling.CollectAndContinue,
            };

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var commandInput = Console.ReadLine();

                    if (commandInput is null) continue;

                    var parts = Regex.Matches(commandInput, @"[\""].+?[\""]|[^ ]+") // todo from tools
                                        .Cast<Match>()
                                        .Select(m => m.Value)
                                        .ToArray();

                    if (parts.Length > 0)
                        parts[0] = parts[0] switch
                        {
                            "l" => "leave",
                            "r" => "reload",
                            "s" => "save",
                            "i" => "info",
                            "v" => "version",
                            _ => parts[0]
                        };

                    console.Execute(parts);
                }
                catch (Exception e)
                {
                    logger.LogWarning(e, "Error while executing command");
                }
            }
        }, cancellationToken);
    }

    public void PrintAvailableCommands()
    {
        Tools.WriteColor(
         "\nAvailable commands:\n"

         + "\n[send] \t=> Send message:"
             + "\n\t[-c] => Chat with according chat ID. Use . to send to all chats"
             + "\n\t[-m] => Message to send. Please use \"\" to indicate message. Markdown formatting allowed"
         + "\nExample: send -c 123456 -m \"Example message\"\n"

         + "\n[register] => Register new chat:"
             + "\n\t[-l] => List of registered chats"
             + "\n\t[-rm] => Remove one specific chat\n"

         + "\n[leave]/[l] => Leave a chat\n"
         + "\n[reload]/[r] => Reload configurations\n"
         + "\n[save]/[s] \t=> Save last data\n"
         + "\n[info]/[i] \t=> Show info about cached chats and users\n"
         + "\n[version]/[v]=> Version of bot"
         , ConsoleColor.Red, false);
    }
}