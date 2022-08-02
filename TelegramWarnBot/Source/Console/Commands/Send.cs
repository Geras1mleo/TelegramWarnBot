namespace TelegramWarnBot;

public class SendCommand : CommandLineApplication, ICommand
{
    private readonly ITelegramBotClientProvider telegramBotClientProvider;
    private readonly ICachedDataContext cachedDataContext;
    private readonly ILogger<SendCommand> logger;

    private readonly CommandOption chatOption;
    private readonly CommandArgument messageArgument;

    public SendCommand(ITelegramBotClientProvider telegramBotClientProvider,
                       ICachedDataContext cachedDataContext,
                       ILogger<SendCommand> logger)
    {
        this.telegramBotClientProvider = telegramBotClientProvider;
        this.cachedDataContext = cachedDataContext;
        this.logger = logger;

        Name = "send";
        Description = "Sending message into one specific chat or all cached chats";

        chatOption = Option("-c | --chat", "Chat with according Chat Id. If not specified, message is sent to all chats", CommandOptionType.SingleValue, c => c.DefaultValue = "all");
        messageArgument = Argument("Message", "Message to send. Please use \"\" to indicate message. Markdown formating allowed", c => c.Accepts().MinLength(3)).IsRequired(allowEmptyStrings: false);
    }

    public int OnExecute()
    {
        var message = messageArgument.Value.Trim('\"');

        var chats = new List<ChatDTO>();

        if (!long.TryParse(chatOption.Value().Trim('\"'), out var chatId))
        {
            chats = cachedDataContext.Chats;
        }
        else
        {
            var chat = cachedDataContext.FindChatById(chatId);

            if (chat is not null)
                chats.Add(chat);
        }

        for (int i = 0; i < chats.Count; i++)
        {
            if (chats[i].Id != 0)
            {
                try
                {
                    telegramBotClientProvider.SendMessageAsync(chats[i].Id, message)
                              .GetAwaiter().GetResult();
                }
                catch (Exception e)
                {
                    logger.LogError("Error occured while sending message into chat: {chat}. \nError message: {error}", chats[i].Name, e.Message);
                }
            }
        }

        logger.LogInformation("Message \"{message}\" sent into chats: {@chats}",
                              message.Truncate(50),
                              chats.Select(c => c.Name + ": " + c.Id));

        return 1;
    }
}
