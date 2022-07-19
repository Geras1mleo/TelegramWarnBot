namespace TelegramWarnBot;

public interface IWarnController
{
    Task<BotResponse> Unwarn(UpdateContext context);
    Task<BotResponse> Warn(UpdateContext context);
    Task<BotResponse> WCount(UpdateContext context);
}

public class WarnController : IWarnController
{
    private readonly IConfigurationContext configurationContext;
    private readonly ICachedDataContext cachedDataContext;
    private readonly IChatHelper chatHelper;
    private readonly IResponseHelper responseHelper;
    private readonly ICommandService commandService;
    private readonly ILogger<WarnController> logger;

    public WarnController(IConfigurationContext configurationContext,
                          ICachedDataContext cachedDataContext,
                          IChatHelper chatHelper,
                          IResponseHelper responseHelper,
                          ICommandService commandService,
                          ILogger<WarnController> logger)
    {
        this.configurationContext = configurationContext;
        this.cachedDataContext = cachedDataContext;
        this.chatHelper = chatHelper;
        this.responseHelper = responseHelper;
        this.commandService = commandService;
        this.logger = logger;
    }

    public async Task<BotResponse> Warn(UpdateContext context)
    {
        var resolve = commandService.ResolveWarnedRoot(context, true);

        if (!resolve.TryPickT0(out WarnedUser warnedUser, out _))
            return new(resolve.AsT1);

        var banned = await commandService.Warn(warnedUser,
                                               context.Update.Message.Chat.Id,
                                               configurationContext.Configuration.DeleteWarnMessage ? context.Update.Message.MessageId : null,
                                               !chatHelper.IsAdmin(context.Update.Message.Chat.Id, warnedUser.Id),
                                               context.Client, context.CancellationToken);

        var warnedUserName = commandService.ResolveMentionedUser(context.Update, context.Bot).AsT0.GetName();

        if (banned)
            logger.LogInformation("[Admin] Banned user {user} from chat {chat}.",
                warnedUserName, context.ChatDTO.Name);
        else
            logger.LogInformation("[Admin] Warned user {user} in chat {chat}. Current: {currentWarns} / {maxWarns}",
                warnedUserName, context.ChatDTO.Name, warnedUser.Warnings, configurationContext.Configuration.MaxWarnings);

        // Notify in chat that user has been warned or banned
        return new(responseHelper.ResolveResponseVariables(context, banned ? configurationContext.Configuration.Captions.BannedSuccessfully
                                                                           : configurationContext.Configuration.Captions.WarnedSuccessfully,
                                                           warnedUser.Id));
    }

    public async Task<BotResponse> Unwarn(UpdateContext context)
    {
        var resolve = commandService.ResolveWarnedRoot(context, false);

        if (!resolve.TryPickT0(out var warnedUser, out _))
            return new(resolve.AsT1);

        string unwarnedUsername = commandService.ResolveMentionedUser(context.Update, context.Bot).AsT0.GetName();

        if (warnedUser.Warnings == 0)
        {
            return new(responseHelper.ResolveResponseVariables(context, configurationContext.Configuration.Captions.UnwarnUserNoWarnings,
                                                               warnedUser.Id));
        }

        warnedUser.Warnings--;

        if (configurationContext.Configuration.DeleteWarnMessage)
            await context.Client.DeleteMessageAsync(context.Update.Message.Chat.Id, context.Update.Message.MessageId, context.CancellationToken);

        await context.Client.UnbanChatMemberAsync(context.Update.Message.Chat.Id, warnedUser.Id, onlyIfBanned: true, cancellationToken: context.CancellationToken);

        return new(responseHelper.ResolveResponseVariables(context, configurationContext.Configuration.Captions.UnwarnedSuccessfully,
                                                           warnedUser.Id));
    }

    public Task<BotResponse> WCount(UpdateContext context)
    {
        var chat = commandService.ResolveChatWarning(context.Update.Message.Chat.Id);

        var resolveUser = commandService.ResolveMentionedUser(context.Update, context.Bot);

        UserDTO user = resolveUser.Match<UserDTO>(
        userDto => userDto,
        result =>
        {
            return result switch
            {
                ResolveMentionedUserResult.UserNotMentioned => context.Update.Message.From.Map(),
                _ => null,
            };
        });

        if (user is null)
        {
            return resolveUser.AsT1 switch
            {
                ResolveMentionedUserResult.UserNotFound => Task.FromResult(new BotResponse(configurationContext.Configuration.Captions.UserNotFound)),
                ResolveMentionedUserResult.BotMention => Task.FromResult(new BotResponse(configurationContext.Configuration.Captions.WCountBotAttempt)),
                ResolveMentionedUserResult.BotSelfMention => Task.FromResult(new BotResponse(configurationContext.Configuration.Captions.WCountBotSelfAttempt)),
                _ => throw new ArgumentException("ResolveMentionedUserResult")
            };
        }

        if (!configurationContext.Configuration.AllowAdminWarnings)
        {
            var isAdmin = chatHelper.IsAdmin(chat.ChatId, user.Id);
            if (isAdmin)
                return Task.FromResult(new BotResponse(configurationContext.Configuration.Captions.WCountAdminAttempt));
        }

        var count = cachedDataContext.Warnings.Find(c => c.ChatId == chat.ChatId)?.WarnedUsers.Find(u => u.Id == user.Id)?.Warnings ?? 0;

        if (count == 0)
        {
            return Task.FromResult(new BotResponse(
                responseHelper.ResolveResponseVariables(context, configurationContext.Configuration.Captions.WCountUserHasNoWarnings, user.Id)));
        }

        return Task.FromResult(new BotResponse(
            responseHelper.ResolveResponseVariables(context, configurationContext.Configuration.Captions.WCountMessage, user.Id)));
    }
}
