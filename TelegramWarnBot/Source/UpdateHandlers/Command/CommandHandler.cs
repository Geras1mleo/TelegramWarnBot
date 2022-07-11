namespace TelegramWarnBot;

public class CommandHandler : Pipe<UpdateContext>
{
    private readonly IConfigurationContext configurationContext;
    private readonly IWarnController warnController;

    public CommandHandler(Func<UpdateContext, Task> next,
                          IConfigurationContext configurationContext,
                          IWarnController warnController) : base(next)
    {
        this.configurationContext = configurationContext;
        this.warnController = warnController;
    }

    public override async Task<Task> Handle(UpdateContext context)
    {
        var method = Tools.ResolveMethod(warnController.GetType(), context.Update.Message.Text.Split(' ')[0][1..]);

        if (method is not null)
        {
            if (!configurationContext.IsChatRegistered(context.Update.Message.Chat.Id))
            {
                await context.Client.SendTextMessageAsync(context.Update.Message.Chat.Id,
                                                           configurationContext.Configuration.Captions.ChatNotRegistered,
                                                           cancellationToken: context.CancellationToken,
                                                           parseMode: ParseMode.Markdown);
            }

            BotResponse response = ((Task<BotResponse>)(method.Invoke(warnController, new object[] { context })))
                                            .GetAwaiter()
                                            .GetResult();

            // If response provided
            if (response is not null)
            {
                await context.Client.SendTextMessageAsync(context.Update.Message.Chat.Id,
                                                        response.Data,
                                                        cancellationToken: context.CancellationToken,
                                                        parseMode: ParseMode.Markdown);

            }
        }

        return next(context);
    }
}