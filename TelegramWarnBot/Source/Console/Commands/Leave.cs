namespace TelegramWarnBot;

public class LeaveCommand : CommandLineApplication, ICommand
{
    private readonly ITelegramBotClientProvider telegramBotClientProvider;
    private readonly ICachedDataContext cachedDataContext;
    private readonly ILogger<LeaveCommand> logger;

    private readonly CommandArgument chatArgument;

    public LeaveCommand(ITelegramBotClientProvider telegramBotClientProvider,
                        ICachedDataContext cachedDataContext,
                        ILogger<LeaveCommand> logger)
    {
        this.telegramBotClientProvider = telegramBotClientProvider;
        this.cachedDataContext = cachedDataContext;
        this.logger = logger;

        Name = "leave";
        Description = "Leave a specific chat";

        chatArgument = Argument("Chat Id", "Chat that bot will leave",
            c => c.Accepts().RegularExpression("^\\\"?\\-?\\d+\"?$", "Not valid chat id"))
                  .IsRequired();
    }

    public int OnExecute()
    {
        long chatId = long.Parse(chatArgument.Value.Trim('\"'));

        telegramBotClientProvider.LeaveChatAsync(chatId).GetAwaiter().GetResult();

        logger.LogInformation("Chat {chat} left successfully!",
                              $"{cachedDataContext.FindChatById(chatId)?.Name}: {chatId}");

        return 1;
    }
}
