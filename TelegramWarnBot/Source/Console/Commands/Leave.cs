namespace TelegramWarnBot;

public class LeaveCommand : CommandLineApplication, ICommand
{
    private readonly IBot bot;
    private readonly CommandArgument chatArgument;

    public LeaveCommand(IBot bot)
    {
        this.bot = bot;

        Name = "leave";
        Description = "Leave a specific chat";

        chatArgument = Argument("Chat Id", "Chat to that bot will leave",
            c => c.Accepts().RegularExpression("^\\\"?\\-?\\d+\"?$", "Not valid chat id"))
                  .IsRequired();
    }

    public int OnExecute()
    {
        bot.Client.LeaveChatAsync(long.Parse(chatArgument.Value.Trim('\"')))
            .GetAwaiter().GetResult();

        return 1;
    }
}
