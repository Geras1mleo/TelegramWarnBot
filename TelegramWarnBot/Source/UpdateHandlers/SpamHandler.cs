namespace TelegramWarnBot;

[RegisteredChat]
[BotAdmin]
public class SpamHandler : Pipe<UpdateContext>
{
    private readonly ICachedDataContext cachedDataContext;
    private readonly IConfigurationContext configurationContext;
    private readonly IDateTimeProvider dateTimeProvider;
    private readonly IInMemoryCachedDataContext inMemoryCachedDataContext;
    private readonly ILogger<SpamHandler> logger;
    private readonly IMessageHelper messageHelper;
    private readonly IResponseHelper responseHelper;
    private readonly IStatsController statsController;

    public SpamHandler(Func<UpdateContext, Task> next,
                       IInMemoryCachedDataContext inMemoryCachedDataContext,
                       IConfigurationContext configurationContext,
                       IMessageHelper messageHelper,
                       IResponseHelper responseHelper,
                       IDateTimeProvider dateTimeProvider,
                       ILogger<SpamHandler> logger,
                       ICachedDataContext cachedDataContext,
                       IStatsController statsController) : base(next)
    {
        this.inMemoryCachedDataContext = inMemoryCachedDataContext;
        this.configurationContext = configurationContext;
        this.messageHelper = messageHelper;
        this.responseHelper = responseHelper;
        this.dateTimeProvider = dateTimeProvider;
        this.logger = logger;
        this.cachedDataContext = cachedDataContext;
        this.statsController = statsController;
    }

    public override Task Handle(UpdateContext context)
    {
        if (!configurationContext.Configuration.DeleteLinksFromNewMembers)
            return next(context);

        var member = inMemoryCachedDataContext.Members.LastOrDefault(m => m.ChatId == context.ChatDTO.Id
                                                                       && m.UserId == context.UserDTO.Id);

        // Member joined less than 24 (or other value from config) hours ago
        var joinedTime = dateTimeProvider.DateTimeNow - (member?.JoinedDate ?? DateTime.MinValue);

        if (joinedTime >= TimeSpan.FromHours(configurationContext.Configuration.NewMemberStatusFromHours))
            return next(context);

        var isTextSpam = context.IsText && (messageHelper.MatchLinkMessage(context.Update.Message)
                                         || messageHelper.MatchCardNumber(context.Text));

        var isMediaSpam = messageHelper.MatchForwardedMedia(context.Update.Message);

        if (!isTextSpam && !isMediaSpam)
            return next(context);

        var deletingTask = responseHelper.DeleteMessageAsync(context);

        logger.LogInformation("[Spam] Message \"{message}\" from {user} in chat {chat} has been deleted",
                              isTextSpam ? context.Text.Truncate(50) : "[media content]",
                              context.UserDTO.GetName(),
                              context.ChatDTO.Name);

        var deletedMessageLog = new DeletedMessageLog
        {
            User = context.UserDTO.GetName(),
            Message = context.Text,
            Time = DateTime.Now
        };

        cachedDataContext.Spam.Add(deletedMessageLog);
        statsController.AddDeletedSpamMessage(deletedMessageLog);

        return deletingTask;
    }
}