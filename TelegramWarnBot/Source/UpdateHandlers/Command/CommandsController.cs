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
        var resolve = commandService.ResolveWarnedRoot(context, true);

        if (!resolve.TryPickT0(out WarnedUser warnedUser, out string errorMessage))
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
        var resolve = commandService.ResolveWarnedRoot(context, false);

        if (!resolve.TryPickT0(out WarnedUser unwarnedUser, out string errorMessage))
        {
            return responseHelper.SendMessageAsync(new()
            {
                Message = errorMessage,
            }, context);
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

        if (configurationContext.Configuration.DeleteWarnMessage)
        {
            await responseHelper.DeleteMessageAsync(context);
        }

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
        var chat = commandService.ResolveChatWarning(context.ChatDTO.Id);

        var resolveUser = commandService.ResolveMentionedUser(context);

        UserDTO mentionedUser = resolveUser.Match<UserDTO>(
        userDto => userDto,
        result =>
        {
            return result switch
            {
                ResolveMentionedUserResult.UserNotMentioned => context.Update.Message.From.Map(),
                _ => null,
            };
        });

        if (mentionedUser is null)
        {
            var response = new ResponseContext
            {
                Message = resolveUser.AsT1 switch
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
            var mentionedUserIsAdmin = chatHelper.IsAdmin(chat.ChatId, mentionedUser.Id);
            if (mentionedUserIsAdmin)
            {
                return responseHelper.SendMessageAsync(new ResponseContext()
                {
                    Message = configurationContext.Configuration.Captions.WCountAdminAttempt
                }, context);
            }
        }

        var warningsCount = cachedDataContext.Warnings.Find(c => c.ChatId == chat.ChatId)?
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
