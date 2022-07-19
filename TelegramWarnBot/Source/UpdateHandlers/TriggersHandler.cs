namespace TelegramWarnBot;

[RegisteredChat]
[TextMessageUpdate]
public class TriggersHandler : Pipe<UpdateContext>
{
    private readonly IConfigurationContext configurationContext;
    private readonly IMessageHelper messageHelper;
    private readonly IResponseHelper responseHelper;

    public TriggersHandler(Func<UpdateContext, Task> next,
                           IConfigurationContext configurationContext,
                           IMessageHelper messageHelper,
                           IResponseHelper responseHelper) : base(next)
    {
        this.configurationContext = configurationContext;
        this.messageHelper = messageHelper;
        this.responseHelper = responseHelper;
    }

    public override async Task<Task> Handle(UpdateContext context)
    {
        foreach (var trigger in configurationContext.Triggers)
        {
            if (trigger.Chat is not null && trigger.Chat != context.Update.Message.Chat.Id)
                continue;

            if (messageHelper.MatchMessage(trigger.Messages, trigger.MatchWholeMessage, trigger.MatchCase, context.Update.Message.Text))
            {
                // Get random response
                var response = trigger.Responses[Random.Shared.Next(trigger.Responses.Length)];

                await responseHelper.SendMessageAsync(new()
                {
                    Message = response
                }, context, context.Update.Message.MessageId);

                // Match only 1 trigger
                return next(context);
            }
        }

        return next(context);
    }
}
