namespace TelegramWarnBot;

[RegisteredChat]
[TextMessageUpdate]
public class SpamHandler : Pipe<UpdateContext>
{
    private readonly ICachedDataContext cachedDataContext;
    private readonly IMessageHelper messageHelper;
    private readonly IResponseHelper responseHelper;

    public SpamHandler(Func<UpdateContext, Task> next,
                       ICachedDataContext cachedDataContext,
                       IMessageHelper messageHelper,
                       IResponseHelper responseHelper) : base(next)
    {
        this.cachedDataContext = cachedDataContext;
        this.messageHelper = messageHelper;
        this.responseHelper = responseHelper;
    }

    public override Task Handle(UpdateContext context)
    {
        if ((context.Update.Message.Entities?.Any(e => e.Type == MessageEntityType.Url || e.Type == MessageEntityType.TextLink
                                              || e.Type == MessageEntityType.TextMention || e.Type == MessageEntityType.Mention) ?? false)
           || messageHelper.MatchCardNumber(context.Update.Message.Text))
        {
            var member = cachedDataContext.Members.FirstOrDefault(m => m.ChatId == context.ChatDTO.Id 
                                                                    && m.UserId == context.Update.Message.From.Id);

            // Member joined less than 24 hours ago
            if (DateTime.Now - (member?.JoinedDate ?? DateTime.MinValue) < TimeSpan.FromHours(24))
            {
                return responseHelper.DeleteMessageAsync(context);
            }
        }

        return next(context);
    }
}
