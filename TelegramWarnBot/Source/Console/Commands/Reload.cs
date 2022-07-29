namespace TelegramWarnBot;

public class ReloadCommand : CommandLineApplication, ICommand
{
    private readonly IConfigurationContext configurationContext;
    private readonly ILogger<ReloadCommand> logger;

    public ReloadCommand(IConfigurationContext configurationContext,
                         ILogger<ReloadCommand> logger)
    {
        this.configurationContext = configurationContext;
        this.logger = logger;

        Name = "reload";
        Description = "Reload configurations (See: Configuration, Triggers and IllegalTriggers)";
    }

    public int OnExecute()
    {
        configurationContext.ReloadConfiguration();
        logger.LogInformation("Configuration reloaded successfully!");
        return 1;
    }
}
