namespace TelegramWarnBot;

public interface ICommandsController
{
    Task<Task> Unwarn(UpdateContext context);
    Task<Task> Warn(UpdateContext context);
    Task WCount(UpdateContext context);
}

public class CommandsController : ICommandsController
{
    private readonly IConfigurationContext configurationContext;
    private readonly ICachedDataContext cachedDataContext;
    private readonly IChatHelper chatHelper;
    private readonly IResponseHelper responseHelper;
    private readonly ICommandService commandService;
    private readonly ILogger<CommandsController> logger;

    public CommandsController(IConfigurationContext configurationContext,
                          ICachedDataContext cachedDataContext,
                          IChatHelper chatHelper,
                          IResponseHelper responseHelper,
                          ICommandService commandService,
                          ILogger<CommandsController> logger)
    {
        this.configurationContext = configurationContext;
        this.cachedDataContext = cachedDataContext;
        this.chatHelper = chatHelper;
        this.responseHelper = responseHelper;
        this.commandService = commandService;
        this.logger = logger;
    }

    public async Task<Task> Warn(UpdateContext context)
    {
        if (!commandService.TryResolveWarnedUser(context, true, out WarnedUser warnedUser, out string errorMessage))
        {
            return responseHelper.SendMessageAsync(new()
            {
                Message = errorMessage,
            }, context);
        }

        var banned = await commandService.Warn(warnedUser, context.ChatDTO.Id,
                                               !chatHelper.IsAdmin(context.Update.Message.Chat.Id, warnedUser.Id),
                                               context);

        if (configurationContext.Configuration.DeleteWarnMessage)
        {
            await responseHelper.DeleteMessageAsync(context);
        }

        LogWarned(banned, context.ChatDTO, warnedUser);

        // Notify in chat that user has been warned or banned
        return responseHelper.SendMessageAsync(new()
        {
            Message = banned ? configurationContext.Configuration.Captions.BannedSuccessfully
                             : configurationContext.Configuration.Captions.WarnedSuccessfully,
            MentionedUserId = warnedUser.Id
        }, context);
    }

    private void LogWarned(bool banned, ChatDTO chat, WarnedUser warnedUser)
    {
        var userName = cachedDataContext.Users.Find(u => u.Id == warnedUser.Id).GetName();

        if (banned)
            logger.LogInformation("[Admin] Banned user {user} from chat {chat}.",
                                   userName, chat.Name);
        else
            logger.LogInformation("[Admin] Warned user {user} in chat {chat}. Warnings: {currentWarns} / {maxWarns}",
                                   userName, chat.Name, warnedUser.Warnings, configurationContext.Configuration.MaxWarnings);
    }

    public async Task<Task> Unwarn(UpdateContext context)
    {
        if (!commandService.TryResolveWarnedUser(context, true, out WarnedUser unwarnedUser, out string errorMessage))
        {
            return responseHelper.SendMessageAsync(new()
            {
                Message = errorMessage,
            }, context);
        }

        if (configurationContext.Configuration.DeleteWarnMessage)
        {
            await responseHelper.DeleteMessageAsync(context);
        }

        if (unwarnedUser.Warnings == 0)
        {
            return responseHelper.SendMessageAsync(new()
            {
                Message = configurationContext.Configuration.Captions.UnwarnUserNoWarnings,
                MentionedUserId = unwarnedUser.Id
            }, context);
        }

        unwarnedUser.Warnings--;

        await context.Client.UnbanChatMemberAsync(context.Update.Message.Chat.Id,
                                                  unwarnedUser.Id,
                                                  onlyIfBanned: true,
                                                  cancellationToken: context.CancellationToken);

        return responseHelper.SendMessageAsync(new()
        {
            Message = configurationContext.Configuration.Captions.UnwarnedSuccessfully,
            MentionedUserId = unwarnedUser.Id
        }, context);
    }

    public Task WCount(UpdateContext context)
    {
        var resolveUser = commandService.TryResolveMentionedUser(context, out UserDTO mentionedUser);

        if (resolveUser == ResolveMentionedUserResult.UserNotMentioned)
        {
            mentionedUser = context.UserDTO;
        }
        else if (resolveUser != ResolveMentionedUserResult.Resolved)
        {
            var response = new ResponseContext
            {
                Message = resolveUser switch
                {
                    ResolveMentionedUserResult.UserNotFound => configurationContext.Configuration.Captions.UserNotFound,
                    ResolveMentionedUserResult.BotMention => configurationContext.Configuration.Captions.WCountBotAttempt,
                    ResolveMentionedUserResult.BotSelfMention => configurationContext.Configuration.Captions.WCountBotSelfAttempt,
                    _ => throw new ArgumentException("ResolveMentionedUserResult")
                }
            };
            return responseHelper.SendMessageAsync(response, context);
        }

        if (!configurationContext.Configuration.AllowAdminWarnings)
        {
            var mentionedUserIsAdmin = chatHelper.IsAdmin(context.ChatDTO.Id, mentionedUser.Id);
            if (mentionedUserIsAdmin)
            {
                return responseHelper.SendMessageAsync(new ResponseContext()
                {
                    Message = configurationContext.Configuration.Captions.WCountAdminAttempt
                }, context);
            }
        }

        var warningsCount = cachedDataContext.Warnings.Find(c => c.ChatId == context.ChatDTO.Id)?
                                             .WarnedUsers.Find(u => u.Id == mentionedUser.Id)?.Warnings ?? 0;

        if (warningsCount == 0)
        {
            return responseHelper.SendMessageAsync(new ResponseContext()
            {
                Message = configurationContext.Configuration.Captions.WCountUserHasNoWarnings,
                MentionedUserId = mentionedUser.Id
            }, context);
        }

        return responseHelper.SendMessageAsync(new ResponseContext()
        {
            Message = configurationContext.Configuration.Captions.WCountMessage,
            MentionedUserId = mentionedUser.Id
        }, context);
    }
}
