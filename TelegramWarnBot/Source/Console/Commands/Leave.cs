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

        telegramBotClientProvider.Client.LeaveChatAsync(chatId).GetAwaiter().GetResult();

        logger.LogInformation("Chat {chat} left successfully!",
                              $"{cachedDataContext.Chats.Find(c => c.Id == chatId)?.Name}: {chatId}"); //todo with function getchatbyid

        return 1;
    }
}
