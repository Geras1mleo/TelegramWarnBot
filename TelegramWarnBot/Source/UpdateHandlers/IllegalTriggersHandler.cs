namespace TelegramWarnBot;

[RegisteredChat]
[TextMessageUpdate]
[BotAdmin]
public class IllegalTriggersHandler : Pipe<UpdateContext>
{
    private readonly ITelegramBotClientProvider telegramBotClientProvider;
    private readonly IConfigurationContext configurationContext;
    private readonly ICachedDataContext cachedDataContext;
    private readonly IMessageHelper messageHelper;
    private readonly IResponseHelper responseHelper;
    private readonly ICommandService commandService;
    private readonly ILogger<IllegalTriggersHandler> logger;

    public IllegalTriggersHandler(Func<UpdateContext, Task> next,
                                  ITelegramBotClientProvider telegramBotClientProvider,
                                  IConfigurationContext configurationContext,
                                  ICachedDataContext cachedDataContext,
                                  IMessageHelper messageHelper,
                                  IResponseHelper responseHelper,
                                  ICommandService commandService,
                                  ILogger<IllegalTriggersHandler> logger) : base(next)
    {
        this.telegramBotClientProvider = telegramBotClientProvider;
        this.configurationContext = configurationContext;
        this.cachedDataContext = cachedDataContext;
        this.messageHelper = messageHelper;
        this.responseHelper = responseHelper;
        this.commandService = commandService;
        this.logger = logger;
    }

    public override async Task<Task> Handle(UpdateContext context)
    {
        foreach (var trigger in configurationContext.IllegalTriggers)
        {
            // Illegal triggers => ignore admins?
            if (trigger.IgnoreAdmins && context.IsSenderAdmin)
                continue;

            // Applicapble in specific chat
            if (trigger.Chat is not null && trigger.Chat != context.ChatDTO.Id)
                continue;

            if (!messageHelper.MatchMessage(trigger.IllegalWords, false, false, context.Text))
                continue;

            logger.LogInformation("Message \"{message}\" from {user} in chat {chat} triggered a IllegalTrigger",
                                  context.Text.Truncate(50),
                                  context.UserDTO.GetName(),
                                  context.ChatDTO.Name);

            await NotifyAdminsAsync(context, trigger);

            if (trigger.DeleteMessage)
            {
                try
                {
                    await responseHelper.DeleteMessageAsync(context);
                }
                catch (Exception e)
                {
                    logger.LogInformation("Could not delete illegal message..\n{message}", e.Message);
                }

                cachedDataContext.Illegal.Add(new DeletedMessageLog { User = context.UserDTO.GetName(), Message = context.Text});

                logger.LogInformation("Illegal message deleted successfully!");
            }

            // Notify but don't warn admins and don't delete message if not allowed in config
            if (trigger.WarnMember)
            {
                if (!context.IsSenderAdmin || configurationContext.Configuration.AllowAdminWarnings)
                {
                    var chatWarning = commandService.ResolveChatWarning(context.ChatDTO.Id);
                    var warnedUser = commandService.ResolveWarnedUser(context.UserDTO.Id, chatWarning);

                    var isBanned = await commandService.Warn(warnedUser,
                                                           chatWarning.ChatId,
                                                           !context.IsSenderAdmin,
                                                           context);

                    LogWarned(isBanned, context.ChatDTO, warnedUser);

                    return responseHelper.SendMessageAsync(new()
                    {
                        Message = isBanned ? configurationContext.Configuration.Captions.IllegalTriggerBanned
                                           : configurationContext.Configuration.Captions.IllegalTriggerWarned,
                        MentionedUserId = warnedUser.Id
                    }, context);
                }
            }

            // Match only 1 trigger
            return next(context);
        }

        return next(context);
    }

    private async Task NotifyAdminsAsync(UpdateContext context, IllegalTrigger trigger)
    {
        var notified = new List<long>();

        foreach (var adminId in trigger.NotifiedAdmins)
        {
            try
            {
                await telegramBotClientProvider.SendMessageAsync(adminId,
                                                                  $"*Illegal message detected!*" +
                                                                  $"\nChat: *{context.ChatDTO.Name}*" +
                                                                  $"\nFrom: {context.UserDTO}" +
                                                                  $"\nSent: {context.Update.Message.Date}" +
                                                                  $"\nContent:",
                                                                  cancellationToken: context.CancellationToken);

                await telegramBotClientProvider.ForwardMessageAsync(adminId, context.ChatDTO.Id,
                                                                    context.Update.Message.MessageId,
                                                                    cancellationToken: context.CancellationToken);

                notified.Add(adminId);
            }
            catch (Exception e)
            {
                logger.LogInformation("Could not notify admin {admin} about an illegal trigger..\n{message}", adminId, e.Message);
            }
        }

        if (notified.Count > 0)
            logger.LogInformation("Notified admins about triggered action: {@admins}", notified);
    }

    private void LogWarned(bool banned, ChatDTO chat, WarnedUser warnedUser)
    {
        var userName = cachedDataContext.FindUserById(warnedUser.Id).GetName();

        if (banned)
            logger.LogInformation("[Auto] Banned user {user} from chat {chat}.",
                                   userName, chat.Name);
        else
            logger.LogInformation("[Auto] Warned user {user} in chat {chat}. Warnings: {currentWarns} / {maxWarns}",
                                   userName, chat.Name, warnedUser.Warnings, configurationContext.Configuration.MaxWarnings);
    }
}
