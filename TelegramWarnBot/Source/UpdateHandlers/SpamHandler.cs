namespace TelegramWarnBot;

public class SpamHandler : Pipe<UpdateContext>
{
    private readonly ICachedDataContext cachedDataContext;
    private readonly IMessageHelper messageHelper;

    public SpamHandler(Func<UpdateContext, Task> next,
                       ICachedDataContext cachedDataContext,
                       IMessageHelper messageHelper) : base(next)
    {
        this.cachedDataContext = cachedDataContext;
        this.messageHelper = messageHelper;
    }

    public override Task Handle(UpdateContext context)
    {
        if ((context.Update.Message.Entities?.Any(e => e.Type == MessageEntityType.Url || e.Type == MessageEntityType.TextLink
                                              || e.Type == MessageEntityType.TextMention || e.Type == MessageEntityType.Mention) ?? false)
           || messageHelper.MatchCardNumber(context.Update.Message.Text))
        {
            var member = cachedDataContext.Members.FirstOrDefault(m => m.ChatId == context.ChatDTO.Id && m.UserId == context.Update.Message.From.Id);

            // Member joined less than 24 hours ago
            if (DateTime.Now - (member?.JoinedDate ?? DateTime.MinValue) < TimeSpan.FromHours(24))
            {
                return context.Client.DeleteMessageAsync(context.ChatDTO.Id, context.Update.Message.MessageId, context.CancellationToken);
            }
        }

        return next(context);
    }
}
