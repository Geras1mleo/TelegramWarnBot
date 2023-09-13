namespace TelegramWarnBot;

public interface ICommand
{
    int OnExecute();
}

public class CommandLineApplicationWithDI : CommandLineApplication
{
    private readonly IServiceProvider serviceProvider;

    public CommandLineApplicationWithDI(IServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider;
        HelpOption("-? | -h | --help");
        RegisterCommands();
    }

    private void RegisterCommands()
    {
        foreach (var command in serviceProvider.GetServices<ICommand>())
        {
            if (command is not CommandLineApplication commandLineApp)
                throw new InvalidCastException("Commands must inherit from ICommand and CommandLineApplication");

            commandLineApp.HelpOption("-? | -h | --help");

            commandLineApp.OnExecute(command.OnExecute);

            AddSubcommand(commandLineApp);
        }
    }
}