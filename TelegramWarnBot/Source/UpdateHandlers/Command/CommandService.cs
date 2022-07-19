namespace TelegramWarnBot;

public interface ICommandService
{
    ChatWarnings ResolveChatWarning(long chatId);
    WarnedUser ResolveWarnedUser(long userId, ChatWarnings chatWarning);
    OneOf<UserDTO, ResolveMentionedUserResult> ResolveMentionedUser(UpdateContext context);
    OneOf<WarnedUser, string> ResolveWarnedRoot(UpdateContext context, bool isWarn);
    Task<bool> Warn(WarnedUser warnedUser, long chatId, bool tryBanUser, UpdateContext context);
}

public class CommandService : ICommandService
{
    private readonly IConfigurationContext configurationContext;
    private readonly ICachedDataContext cachedDataContext;
    private readonly IChatHelper chatHelper;

    public CommandService(IConfigurationContext configurationContext,
                          ICachedDataContext cachedDataContext,
                          IChatHelper chatHelper)
    {
        this.configurationContext = configurationContext;
        this.cachedDataContext = cachedDataContext;
        this.chatHelper = chatHelper;
    }

    /// <summary>
    /// WarnedUser should not be an admin
    /// </summary>
    /// <returns>Whether user is banned from chat</returns>
    public async Task<bool> Warn(WarnedUser warnedUser, long chatId, bool tryBanUser, UpdateContext context)
    {
        warnedUser.Warnings = Math.Clamp(warnedUser.Warnings + 1, 0,
                                         configurationContext.Configuration.MaxWarnings);

        // If not reached max warnings 
        if (warnedUser.Warnings < configurationContext.Configuration.MaxWarnings)
        {
            return false;
        }

        // Max warnings reached
        if (tryBanUser)
        {
            await context.Client.BanChatMemberAsync(chatId,
                                                    warnedUser.Id,
                                                    cancellationToken: context.CancellationToken);
            return true;
        }

        // This reaches only when admin got max warnings but bot cannot ban him...
        return false;
    }

    /// <summary>
    /// return user or error message that has to be returned
    /// </summary>
    /// <param name="client"></param>
    /// <param name="update"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public OneOf<WarnedUser, string> ResolveWarnedRoot(UpdateContext context, bool isWarn)
    {
        if (!context.IsSenderAdmin)
            return configurationContext.Configuration.Captions.UserNoPermissions;

        if (!context.IsBotAdmin)
            return configurationContext.Configuration.Captions.BotHasNoPermissions;

        var resolve = ResolveMentionedUser(context);

        // Didn't find the user => return reason 
        if (!resolve.TryPickT0(out UserDTO user, out _))
        {
            return resolve.AsT1 switch
            {
                ResolveMentionedUserResult.UserNotMentioned => configurationContext.Configuration.Captions.UserNotSpecified,
                ResolveMentionedUserResult.UserNotFound => configurationContext.Configuration.Captions.UserNotFound,
                ResolveMentionedUserResult.BotMention => isWarn ? configurationContext.Configuration.Captions.WarnBotAttempt
                                                                : configurationContext.Configuration.Captions.UnwarnBotAttempt,
                ResolveMentionedUserResult.BotSelfMention => isWarn ? configurationContext.Configuration.Captions.WarnBotSelfAttempt
                                                                    : configurationContext.Configuration.Captions.UnwarnBotSelfAttempt,
                _ => throw new ArgumentException("ResolveMentionedUserResult"),
            };
        }

        var isAdmin = chatHelper.IsAdmin(context.Update.Message.Chat.Id, user.Id);

        // warn/unwarn admin disabled
        if (isAdmin && !configurationContext.Configuration.AllowAdminWarnings)
        {
            return isWarn ? configurationContext.Configuration.Captions.WarnAdminAttempt
                          : configurationContext.Configuration.Captions.UnwarnAdminAttempt;
        }

        var chatWarnings = ResolveChatWarning(context.Update.Message.Chat.Id);

        return ResolveWarnedUser(user.Id, chatWarnings);
    }

    public WarnedUser ResolveWarnedUser(long userId, ChatWarnings chatWarning)
    {
        var warnedUser = chatWarning.WarnedUsers.FirstOrDefault(u => u.Id == userId);
        if (warnedUser is null)
        {
            warnedUser = new()
            {
                Id = userId,
                Warnings = 0
            };
            chatWarning.WarnedUsers.Add(warnedUser);
        }
        return warnedUser;
    }

    public ChatWarnings ResolveChatWarning(long chatId)
    {
        var chatWarning = cachedDataContext.Warnings.FirstOrDefault(c => c.ChatId == chatId);
        if (chatWarning is null)
        {
            chatWarning = new()
            {
                ChatId = chatId,
                WarnedUsers = new List<WarnedUser>()
            };
            cachedDataContext.Warnings.Add(chatWarning);
        }
        return chatWarning;
    }

    // todo TryResolveMentionedUser
    /// <summary>
    /// return user or error message that has to be returned
    /// </summary>
    /// <param name="update"></param>
    /// <returns></returns>
    public OneOf<UserDTO, ResolveMentionedUserResult> ResolveMentionedUser(UpdateContext context)
    {
        User user = null;

        if (context.Update.Message.Entities?.Length >= 2)
        {
            if (context.Update.Message.Entities[1].Type == MessageEntityType.Mention
             && context.Update.Message?.EntityValues is not null)
            {
                var mentionedUser = context.Update.Message.EntityValues.ElementAt(1)[1..];
                var userDto = cachedDataContext.Users.FirstOrDefault(u => u.Username == mentionedUser);

                if (userDto is not null)
                {
                    user = userDto.Map();
                }
            }
            else if (context.Update.Message.Entities[1].Type == MessageEntityType.TextMention
                  && context.Update.Message.Entities[1].User is not null)
            {
                user = context.Update.Message.Entities[1].User;
            }
            // Second entity must be a mention
            else
                return ResolveMentionedUserResult.UserNotMentioned;
        }
        // If didn't mention user in message => look into replied message
        else if (context.Update.Message.ReplyToMessage?.From is not null)
        {
            user = context.Update.Message.ReplyToMessage?.From;
        }
        // Didn't mention and didn't replied
        else
        {
            return ResolveMentionedUserResult.UserNotMentioned;
        }

        if (user is null)
        {
            return ResolveMentionedUserResult.UserNotFound;
        }

        // Mentioned bot itself
        if (user.Id == context.Bot.Id)
        {
            return ResolveMentionedUserResult.BotSelfMention;
        }

        // Mentioned other bot
        if (user.IsBot)
        {
            return ResolveMentionedUserResult.BotMention;
        }

        return user.Map();
    }
}
