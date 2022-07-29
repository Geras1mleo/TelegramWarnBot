namespace TelegramWarnBot;

public class SaveCommand : CommandLineApplication, ICommand
{
    private readonly ICachedDataContext cachedDataContext;
    private readonly ILogger<SaveCommand> logger;

    public SaveCommand(ICachedDataContext cachedDataContext,
                       ILogger<SaveCommand> logger)
    {
        this.cachedDataContext = cachedDataContext;
        this.logger = logger;

        Name = "save";
        Description = "Save last cached data to json files (See: /Data/)";
    }

    public int OnExecute()
    {
        cachedDataContext.SaveData();
        logger.LogInformation("Data saved successfully!");
        return 1;
    }
}
