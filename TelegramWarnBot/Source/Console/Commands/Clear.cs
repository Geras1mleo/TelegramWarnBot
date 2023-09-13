namespace TelegramWarnBot;

public class ClearCommand : CommandLineApplication, ICommand
{
    public ClearCommand()
    {
        Name = "clear";
    }

    public int OnExecute()
    {
        Console.Clear();
        return 1;
    }
}