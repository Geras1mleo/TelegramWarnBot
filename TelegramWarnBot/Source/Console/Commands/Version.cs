namespace TelegramWarnBot;

public class VersionCommand : CommandLineApplication, ICommand
{
    public VersionCommand()
    {
        Name = "version";
        Description = "Version of bot";
    }

    public int OnExecute()
    {
        Tools.WriteColor($"[Version: {Assembly.GetEntryAssembly().GetName().Version}]", ConsoleColor.Yellow, false);
        return 1;
    }
}
