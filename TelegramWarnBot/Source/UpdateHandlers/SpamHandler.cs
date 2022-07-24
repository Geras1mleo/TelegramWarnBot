namespace TelegramWarnBot;

[RegisteredChat]
[TextMessageUpdate]
public class SpamHandler : Pipe<UpdateContext>
{
    private readonly ICachedDataContext cachedDataContext;
    private readonly IMessageHelper messageHelper;
    private readonly IResponseHelper responseHelper;
    private readonly IDateTimeProvider dateTimeProvider;

    public SpamHandler(Func<UpdateContext, Task> next,
                       ICachedDataContext cachedDataContext,
                       IMessageHelper messageHelper,
                       IResponseHelper responseHelper,
                       IDateTimeProvider dateTimeProvider) : base(next)
    {
        this.cachedDataContext = cachedDataContext;
        this.messageHelper = messageHelper;
        this.responseHelper = responseHelper;
        this.dateTimeProvider = dateTimeProvider;
    }

    public override Task Handle(UpdateContext context)
    {
        if (messageHelper.MatchLinkMessage(context.Update.Message) || messageHelper.MatchCardNumber(context.Update.Message.Text))
        {
            var member = cachedDataContext.Members.FirstOrDefault(m => m.ChatId == context.ChatDTO.Id
                                                                    && m.UserId == context.UserDTO.Id);

            // Member joined less than 24 hours ago
            if (dateTimeProvider.DateTimeNow - (member?.JoinedDate ?? DateTime.MinValue) < TimeSpan.FromHours(24))
            {
                return responseHelper.DeleteMessageAsync(context);
            }
        }

        return next(context);
    }
}
