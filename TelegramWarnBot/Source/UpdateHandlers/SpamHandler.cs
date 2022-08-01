namespace TelegramWarnBot;

[RegisteredChat]
[TextMessageUpdate]
[BotAdmin]
public class SpamHandler : Pipe<UpdateContext>
{
    private readonly IInMemoryCachedDataContext inMemoryCachedDataContext;
    private readonly IConfigurationContext configurationContext;
    private readonly IMessageHelper messageHelper;
    private readonly IResponseHelper responseHelper;
    private readonly IDateTimeProvider dateTimeProvider;
    private readonly ILogger<SpamHandler> logger;

    public SpamHandler(Func<UpdateContext, Task> next,
                       IInMemoryCachedDataContext inMemoryCachedDataContext,
                       IConfigurationContext configurationContext,
                       IMessageHelper messageHelper,
                       IResponseHelper responseHelper,
                       IDateTimeProvider dateTimeProvider,
                       ILogger<SpamHandler> logger) : base(next)
    {
        this.inMemoryCachedDataContext = inMemoryCachedDataContext;
        this.configurationContext = configurationContext;
        this.messageHelper = messageHelper;
        this.responseHelper = responseHelper;
        this.dateTimeProvider = dateTimeProvider;
        this.logger = logger;
    }

    public override Task Handle(UpdateContext context)
    {
        if (!configurationContext.Configuration.DeleteLinksFromNewMembers)
            return next(context);

        if (messageHelper.MatchLinkMessage(context.Update.Message) || messageHelper.MatchCardNumber(context.Update.Message.Text))
        {
            var member = inMemoryCachedDataContext.Members.LastOrDefault(m => m.ChatId == context.ChatDTO.Id
                                                                   && m.UserId == context.UserDTO.Id);

            // Member joined less than 24 (or other value from config) hours ago
            var joinedTime = dateTimeProvider.DateTimeNow - (member?.JoinedDate ?? DateTime.MinValue);

            if (joinedTime < TimeSpan.FromHours(configurationContext.Configuration.NewMemberStatusFromHours))
            {
                var deletingTask = responseHelper.DeleteMessageAsync(context);

                logger.LogInformation("Message \"{message}\" from {user} in chat {chat} has been considered as spam and deleted",
                                      context.Update.Message.Text.Truncate(50),
                                      context.UserDTO.GetName(),
                                      context.ChatDTO.Name);

                return deletingTask;
            }
        }

        return next(context);
    }
}
