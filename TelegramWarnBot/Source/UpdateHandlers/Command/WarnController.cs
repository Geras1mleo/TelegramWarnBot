namespace TelegramWarnBot;

public class WarnController
{
    private readonly ConfigurationContext configurationContext;
    private readonly CachedDataContext cachedDataContext;
    private readonly ChatHelper chatHelper;
    private readonly ResponseHelper responseHelper;
    private readonly CommandService commandService;

    public WarnController(ConfigurationContext configurationContext,
                          CachedDataContext cachedDataContext,
                          ChatHelper chatHelper,
                          ResponseHelper responseHelper,
                          CommandService commandService)
    {
        this.configurationContext = configurationContext;
        this.cachedDataContext = cachedDataContext;
        this.chatHelper = chatHelper;
        this.responseHelper = responseHelper;
        this.commandService = commandService;
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

        // Notify in chat that user has been warned or banned
        return new(responseHelper.ResolveResponseVariables(banned ? configurationContext.Configuration.Captions.BannedSuccessfully
                                                                  : configurationContext.Configuration.Captions.WarnedSuccessfully,
                                                           warnedUser, commandService.ResolveMentionedUser(context.Update, context.Bot).AsT0.Name));
    }

    public async Task<BotResponse> Unwarn(UpdateContext context)
    {
        var resolve = commandService.ResolveWarnedRoot(context, false);

        if (!resolve.TryPickT0(out var warnedUser, out _))
            return new(resolve.AsT1);

        if (warnedUser.Warnings == 0)
        {
            return new(responseHelper.ResolveResponseVariables(configurationContext.Configuration.Captions.UserUnwarnNoWarnings,
                                                               warnedUser, commandService.ResolveMentionedUser(context.Update, context.Bot).AsT0.Name));
        }

        warnedUser.Warnings--;

        if (configurationContext.Configuration.DeleteWarnMessage)
            await context.Client.DeleteMessageAsync(context.Update.Message.Chat.Id, context.Update.Message.MessageId, context.CancellationToken);

        await context.Client.UnbanChatMemberAsync(context.Update.Message.Chat.Id, warnedUser.Id, onlyIfBanned: true, cancellationToken: context.CancellationToken);

        return new(responseHelper.ResolveResponseVariables(configurationContext.Configuration.Captions.UnwarnedSuccessfully,
                                                           warnedUser, commandService.ResolveMentionedUser(context.Update, context.Bot).AsT0.Name));
    }

    public Task<BotResponse> WCount(UpdateContext context)
    {
        var chat = commandService.ResolveChatWarning(context.Update.Message.Chat.Id, cachedDataContext.Warnings);

        var resolveUser = commandService.ResolveMentionedUser(context.Update, context.Bot);

        UserDTO user = resolveUser.Match<UserDTO>(
        userDto => userDto,
        result =>
        {
            return result switch
            {
                ResolveMentionedUserResult.UserNotMentioned =>
                user = new()
                {
                    Id = context.Update.Message.From.Id,
                    Name = context.Update.Message.From.GetFullName(),
                },
                _ => null,
            };
        });

        if (user is null)
        {
            return resolveUser.AsT1 switch
            {
                ResolveMentionedUserResult.UserNotFound => Task.FromResult(new BotResponse(configurationContext.Configuration.Captions.UserNotFound)),
                ResolveMentionedUserResult.BotMention => Task.FromResult(new BotResponse(configurationContext.Configuration.Captions.WarningsCountBotMention)),
                ResolveMentionedUserResult.BotSelfMention => Task.FromResult(new BotResponse(configurationContext.Configuration.Captions.WarningsCountBotSelfMention)),
                _ => throw new ArgumentException("ResolveMentionedUserResult")
            };
        }

        if (!configurationContext.Configuration.AllowAdminWarnings)
        {
            var isAdmin = chatHelper.IsAdmin(chat.ChatId, user.Id);
            if (isAdmin)
                return Task.FromResult(new BotResponse(configurationContext.Configuration.Captions.WarningsCountAdminNotAllowed));
        }

        var count = cachedDataContext.Warnings.Find(c => c.ChatId == chat.ChatId)?.WarnedUsers.Find(u => u.Id == user.Id)?.Warnings ?? 0;

        if (count == 0)
        {
            return Task.FromResult(new BotResponse(responseHelper.ResolveResponseVariables(configurationContext.Configuration.Captions.WarningsCountUserHasNoWarnings,
                                                                                            user, 0)));
        }

        return Task.FromResult(new BotResponse(responseHelper.ResolveResponseVariables(configurationContext.Configuration.Captions.WarningsCount,
                                                                                        user, count)));
    }

    public async Task<BotResponse> Update(UpdateContext context)
    {
        var chat = cachedDataContext.Chats.Find(c => c.Id == context.Update.Message.Chat.Id);

        if (chat?.Admins is null)
        {
            cachedDataContext.CacheChat(context.Update.Message.Chat, await chatHelper.GetAdminsAsync(context.Client, context.Update.Message.Chat.Id, context.CancellationToken));
        }
        else
        {
            chat.Admins = await chatHelper.GetAdminsAsync(context.Client, context.Update.Message.Chat.Id, context.CancellationToken);
        }

        return new BotResponse("Admins updated successfully!");
    }
}
