﻿namespace TelegramWarnBot;

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
        Description = "Save last cached data to json files (See: Users, Chats and ChatWarnings)";
    }

    public int OnExecute()
    {
        cachedDataContext.SaveData();
        logger.LogInformation("Cached data saved successfully!");
        return 1;
    }
}