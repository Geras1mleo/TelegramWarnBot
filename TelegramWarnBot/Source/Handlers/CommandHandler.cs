using Autofac;

namespace TelegramWarnBot;

public class CommandHandler : Pipe<TelegramUpdateContext>
{
    private readonly ConfigurationContext configurationContext;
    private readonly UpdateHelper updateHelper;
    private readonly WarnController warnController;

    public CommandHandler(Func<TelegramUpdateContext, Task> next,
                          ConfigurationContext configurationContext,
                          UpdateHelper updateHelper,
                          WarnController warnController) : base(next)
    {
        this.configurationContext = configurationContext;
        this.updateHelper = updateHelper;
        this.warnController = warnController;
    }

    public override async Task<Task> Handle(TelegramUpdateContext context)
    {
        var method = Tools.ResolveMethod(typeof(WarnController), context.Update.Message.Text.Split(' ')[0][1..]);

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