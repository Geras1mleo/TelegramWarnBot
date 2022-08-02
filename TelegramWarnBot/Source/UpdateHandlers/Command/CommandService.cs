using System.Diagnostics.CodeAnalysis;

namespace TelegramWarnBot;

public interface ICommandService
{
    ChatWarnings ResolveChatWarning(long chatId);
    WarnedUser ResolveWarnedUser(long userId, ChatWarnings chatWarning);
    ResolveMentionedUserResult TryResolveMentionedUser(UpdateContext context, out UserDTO userDto);
    public bool TryResolveWarnedUser(UpdateContext context, bool isWarn, [NotNullWhen(true)] out WarnedUser warnedUser, [NotNullWhen(false)] out string errorMessage);
    Task<bool> Warn(WarnedUser warnedUser, long chatId, bool tryBanUser, UpdateContext context);
}

public class CommandService : ICommandService
{
    private readonly ITelegramBotClientProvider telegramBotClientProvider;
    private readonly IConfigurationContext configurationContext;
    private readonly ICachedDataContext cachedDataContext;
    private readonly IChatHelper chatHelper;

    public CommandService(ITelegramBotClientProvider telegramBotClientProvider,
                          IConfigurationContext configurationContext,
                          ICachedDataContext cachedDataContext,
                          IChatHelper chatHelper)
    {
        this.telegramBotClientProvider = telegramBotClientProvider;
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
            await telegramBotClientProvider.BanChatMemberAsync(chatId, warnedUser.Id,
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
    public bool TryResolveWarnedUser(UpdateContext context, bool isWarn, [NotNullWhen(true)] out WarnedUser warnedUser, [NotNullWhen(false)] out string errorMessage)
    {
        warnedUser = null;
        errorMessage = null;

        if (!context.IsSenderAdmin)
        {
            errorMessage = configurationContext.Configuration.Captions.UserNoPermissions;
            return false;
        }

        if (!context.IsBotAdmin)
        {
            errorMessage = configurationContext.Configuration.Captions.BotHasNoPermissions;
            return false;
        }

        var resolveUser = TryResolveMentionedUser(context, out UserDTO mentionedUser);

        // Didn't find the user => return reason 
        if (resolveUser != ResolveMentionedUserResult.Resolved)
        {
            errorMessage = resolveUser switch
            {
                ResolveMentionedUserResult.UserNotMentioned => configurationContext.Configuration.Captions.InvalidOperation,
                ResolveMentionedUserResult.UserNotFound => configurationContext.Configuration.Captions.UserNotFound,
                ResolveMentionedUserResult.BotMention => isWarn ? configurationContext.Configuration.Captions.WarnBotAttempt
                                                                : configurationContext.Configuration.Captions.UnwarnBotAttempt,
                ResolveMentionedUserResult.BotSelfMention => isWarn ? configurationContext.Configuration.Captions.WarnBotSelfAttempt
                                                                    : configurationContext.Configuration.Captions.UnwarnBotSelfAttempt,
                _ => throw new ArgumentException("ResolveMentionedUserResult"),
            };
            return false;
        }

        var mentionedUserIsAdmin = chatHelper.IsAdmin(context.Update.Message.Chat.Id, mentionedUser.Id);

        // warn/unwarn admin disabled
        if (mentionedUserIsAdmin && !configurationContext.Configuration.AllowAdminWarnings)
        {
            errorMessage = isWarn ? configurationContext.Configuration.Captions.WarnAdminAttempt
                                  : configurationContext.Configuration.Captions.UnwarnAdminAttempt;
            return false;
        }

        var chatWarnings = ResolveChatWarning(context.Update.Message.Chat.Id);
        warnedUser = ResolveWarnedUser(mentionedUser.Id, chatWarnings);

        return true;
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
        var chatWarning = cachedDataContext.FindWarningByChatId(chatId);
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

    /// <summary>
    /// return user or error message that has to be responded
    /// </summary>
    /// <param name="update"></param>
    /// <returns></returns>
    public ResolveMentionedUserResult TryResolveMentionedUser(UpdateContext context, out UserDTO userDto)
    {
        userDto = null;
        User user = null;

        if (context.Update.Message.Entities?.Length >= 2)
        {
            if (context.Update.Message.Entities[1].Type == MessageEntityType.Mention
             && context.Update.Message.EntityValues is not null)
            {
                var mentionedUsername = context.Update.Message.EntityValues.ElementAt(1)[1..];
                var mentionedUserDto = cachedDataContext.Users.Find(u => u.Username == mentionedUsername);

                if (mentionedUserDto is not null)
                {
                    user = mentionedUserDto.Map();
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
            user = context.Update.Message.ReplyToMessage.From;
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

        userDto = user.Map();

        return ResolveMentionedUserResult.Resolved;
    }
}
