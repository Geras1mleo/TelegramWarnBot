namespace TelegramWarnBot;

public class TriggersHandler : Pipe<TelegramUpdateContext>
{
    private readonly ConfigurationContext configurationContext;
    private readonly UpdateHelper updateHelper;

    public TriggersHandler(Func<TelegramUpdateContext, Task> next,
                           ConfigurationContext configurationContext,
                           UpdateHelper updateHelper) : base(next)
    {
        this.configurationContext = configurationContext;
        this.updateHelper = updateHelper;
    }

    public override async Task<Task> Handle(TelegramUpdateContext context)
    {
        foreach (var trigger in configurationContext.Triggers)
        {
            if (trigger.Chat is not null && trigger.Chat != context.Update.Message.Chat.Id)
                continue;

            if (updateHelper.MatchMessage(trigger.Messages, trigger.MatchWholeMessage, trigger.MatchCase, context.Update.Message.Text))
            {
                // Get random response
                var response = trigger.Responses[Random.Shared.Next(trigger.Responses.Length)];

                await context.Client.SendTextMessageAsync(context.Update.Message.Chat.Id,
                                                          response,
                                                          replyToMessageId: context.Update.Message.MessageId,
                                                          cancellationToken: context.CancellationToken,
                                                          parseMode: ParseMode.Markdown);
                // Match only 1 trigger
                return next(context);
            }
        }

        return next(context);
    }
}
