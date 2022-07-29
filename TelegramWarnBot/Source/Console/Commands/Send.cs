namespace TelegramWarnBot;

public class SendCommand : CommandLineApplication, ICommand
{
    private readonly IBot bot;
    private readonly ICachedDataContext cachedDataContext;

    private readonly CommandOption chatOption;
    private readonly CommandArgument messageArgument;

    public SendCommand(IBot bot,
                       ICachedDataContext cachedDataContext)
    {
        this.bot = bot;
        this.cachedDataContext = cachedDataContext;

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
            var chat = cachedDataContext.Chats.Find(c => c.Id == chatId);

            if (chat is not null)
                chats.Add(chat);
        }

        var tasks = new List<Task>();

        int sentCount = 0;
        for (int i = 0; i < chats.Count; i++)
        {
            if (chats[i].Id != 0)
            {
                tasks.Add(bot.Client.SendTextMessageAsync(chats[i].Id, message,
                                                          parseMode: ParseMode.Markdown));
                sentCount++;
            }
        }

        Task.WhenAll(tasks).GetAwaiter().GetResult();

        Tools.WriteColor($"[Messages sent: {sentCount}]", ConsoleColor.Yellow, true);

        return 1;
    }
}
