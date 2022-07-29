namespace TelegramWarnBot;

[RegisteredChat]
[TextMessageUpdate]
[BotAdmin]
public class SpamHandler : Pipe<UpdateContext>
{
    private readonly ICachedDataContext cachedDataContext;
    private readonly IConfigurationContext configurationContext;
    private readonly IMessageHelper messageHelper;
    private readonly IResponseHelper responseHelper;
    private readonly IDateTimeProvider dateTimeProvider;

    public SpamHandler(Func<UpdateContext, Task> next,
                       ICachedDataContext cachedDataContext,
                       IConfigurationContext configurationContext,
                       IMessageHelper messageHelper,
                       IResponseHelper responseHelper,
                       IDateTimeProvider dateTimeProvider) : base(next)
    {
        this.cachedDataContext = cachedDataContext;
        this.configurationContext = configurationContext;
        this.messageHelper = messageHelper;
        this.responseHelper = responseHelper;
        this.dateTimeProvider = dateTimeProvider;
    }

    public override Task Handle(UpdateContext context)
    {
        if (!configurationContext.Configuration.DeleteLinksFromNewMembers)
            return next(context);

        if (messageHelper.MatchLinkMessage(context.Update.Message) || messageHelper.MatchCardNumber(context.Update.Message.Text))
        {
            var member = cachedDataContext.Members.FirstOrDefault(m => m.ChatId == context.ChatDTO.Id
                                                                    && m.UserId == context.UserDTO.Id);

            // Member joined less than 24 (or other value from config) hours ago
            var joinedTime = dateTimeProvider.DateTimeNow - (member?.JoinedDate ?? DateTime.MinValue);

            if (joinedTime < TimeSpan.FromHours(configurationContext.Configuration.NewMemberStatusFromHours))
            {
                return responseHelper.DeleteMessageAsync(context);
            }
        }

        return next(context);
    }
}
