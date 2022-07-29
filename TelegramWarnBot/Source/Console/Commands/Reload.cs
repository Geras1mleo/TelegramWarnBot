namespace TelegramWarnBot;

public class ReloadCommand : CommandLineApplication, ICommand
{
    private readonly IConfigurationContext configurationContext;

    public ReloadCommand(IConfigurationContext configurationContext)
    {
        this.configurationContext = configurationContext;

        Name = "reload";
        Description = "Reload configurations";
    }

    public int OnExecute()
    {
        configurationContext.ReloadConfiguration();
        Tools.WriteColor("[Configuration reloaded successfully!]", ConsoleColor.Green, true);

        return 1;
    }
}
