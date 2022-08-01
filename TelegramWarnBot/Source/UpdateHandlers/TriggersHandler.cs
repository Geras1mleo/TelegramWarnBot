namespace TelegramWarnBot;

[RegisteredChat]
[TextMessageUpdate]
public class TriggersHandler : Pipe<UpdateContext>
{
    private readonly IConfigurationContext configurationContext;
    private readonly IInMemoryCachedDataContext inMemoryCachedDataContext;
    private readonly IMessageHelper messageHelper;
    private readonly IResponseHelper responseHelper;
    private readonly ILogger<TriggersHandler> logger;

    public TriggersHandler(Func<UpdateContext, Task> next,
                           IConfigurationContext configurationContext,
                           IInMemoryCachedDataContext inMemoryCachedDataContext,
                           IMessageHelper messageHelper,
                           IResponseHelper responseHelper,
                           ILogger<TriggersHandler> logger) : base(next)
    {
        this.configurationContext = configurationContext;
        this.inMemoryCachedDataContext = inMemoryCachedDataContext;
        this.messageHelper = messageHelper;
        this.responseHelper = responseHelper;
        this.logger = logger;
    }

    public override async Task<Task> Handle(UpdateContext context)
    {
        foreach (var trigger in configurationContext.Triggers)
        {
            if (trigger.Chat is not null && trigger.Chat != context.ChatDTO.Id)
                continue;

            if (messageHelper.MatchMessage(trigger.Messages, trigger.MatchWholeMessage, trigger.MatchCase, context.Update.Message.Text))
            {
                // Get random response
                var response = trigger.Responses[Random.Shared.Next(trigger.Responses.Length)];

                await responseHelper.SendMessageAsync(new()
                {
                    Message = response
                }, context, context.Update.Message.MessageId);

                logger.LogInformation("Message \"{message}\" from {user} in chat {chat} triggered a Trigger. Bot responded with:\"{response}\"",
                                      context.Update.Message.Text.Truncate(50),
                                      context.UserDTO.GetName(),
                                      context.ChatDTO.Name,
                                      response.Truncate(50));

                // Match only 1 trigger
                return next(context);
            }
        }

        return next(context);
    }
}
