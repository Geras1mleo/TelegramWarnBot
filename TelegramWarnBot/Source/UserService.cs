namespace TelegramWarnBot;

public class UserService
{
    /// <summary>
    /// WarnedUser should not be an admin
    /// </summary>
    /// <returns>Whether user is banned from chat</returns>
    public async Task<bool> Warn(WarnedUser warnedUser, long chatId, int? deleteMessageId, bool tryBanUser, ITelegramBotClient client, CancellationToken cancellationToken)
    {
        warnedUser.Warnings = Math.Clamp(warnedUser.Warnings + 1, 0, IOHandler.GetConfiguration().MaxWarnings);

        if (deleteMessageId is not null)
            await client.DeleteMessageAsync(chatId, deleteMessageId.Value, cancellationToken);

        // If not reached max warnings 
        if (warnedUser.Warnings < IOHandler.GetConfiguration().MaxWarnings)
        {
            return false;
        }

        // Max warnings reached
        if (tryBanUser)
        {
            await client.BanChatMemberAsync(chatId, warnedUser.Id, cancellationToken: cancellationToken);
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
    public async Task<OneOf<(WarnedUser user, bool isAdmin), string>> ResolveWarnedRoot(ITelegramBotClient client, Update update, bool isWarn, CancellationToken cancellationToken)
    {
        if (!await IsAdmin(client, update.Message.Chat.Id, update.Message.From.Id, cancellationToken))
            return IOHandler.GetConfiguration().Captions.UserNoPermissions;

        if (!await IsAdmin(client, update.Message.Chat.Id, Bot.User.Id, cancellationToken))
            return IOHandler.GetConfiguration().Captions.BotHasNoPermissions;

        var resolve = ResolveMentionedUser(update);

        // Didn't find the user => return reason 
        if (!resolve.TryPickT0(out UserDTO user, out _))
        {
            return resolve.AsT1 switch
            {
                ResolveMentionedUserResult.UserNotMentioned => IOHandler.GetConfiguration().Captions.UserNotSpecified,
                ResolveMentionedUserResult.UserNotFound => IOHandler.GetConfiguration().Captions.UserNotFound,
                ResolveMentionedUserResult.BotMention => isWarn ? IOHandler.GetConfiguration().Captions.BotWarnAttempt
                                                                : IOHandler.GetConfiguration().Captions.BotUnwarnAttempt,
                ResolveMentionedUserResult.BotSelfMention => isWarn ? IOHandler.GetConfiguration().Captions.BotSelfWarnAttempt
                                                                    : IOHandler.GetConfiguration().Captions.BotSelfUnwarnAttempt,
                _ => throw new ArgumentException("ResolveMentionedUserResult"),
            };
        }

        var isAdmin = await IsAdmin(client, update.Message.Chat.Id, user.Id, cancellationToken);

        // warn/unwarn admin disabled
        if (isAdmin && !IOHandler.GetConfiguration().AllowAdminWarnings)
        {
            return isWarn ? IOHandler.GetConfiguration().Captions.AdminWarnAttempt
                          : IOHandler.GetConfiguration().Captions.AdminUnwarnAttempt;
        }

        var warnings = IOHandler.GetWarnings();
        var chat = ResolveChat(update, warnings);

        return (ResolveWarnedUser(user.Id, chat), isAdmin);
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

    public ChatWarnings ResolveChat(Update update, List<ChatWarnings> warnings)
    {
        var chat = warnings.FirstOrDefault(c => c.ChatId == update.Message.Chat.Id);
        if (chat is null)
        {
            chat = new()
            {
                ChatId = update.Message.Chat.Id,
                WarnedUsers = new List<WarnedUser>()
            };
            warnings.Add(chat);
        }
        return chat;
    }

    /// <summary>
    /// return user or error message that has to be returned
    /// </summary>
    /// <param name="update"></param>
    /// <returns></returns>
    public OneOf<UserDTO, ResolveMentionedUserResult> ResolveMentionedUser(Update update)
    {
        User user = null;

        if (update.Message.Entities?.Length >= 2)
        {
            if (update.Message.Entities[1].Type == MessageEntityType.Mention
             && update.Message?.EntityValues is not null)
            {
                var mentionedUser = update.Message.EntityValues.ElementAt(1)[1..].ToLower();
                var userDto = IOHandler.GetUsers().FirstOrDefault(u => u.Username == mentionedUser);

                if (userDto is not null)
                {
                    user = new()
                    {
                        Id = userDto.Id,
                        FirstName = userDto.Name,
                        Username = userDto.Username,
                    };
                }
            }
            else if (update.Message.Entities[1].Type == MessageEntityType.TextMention
                  && update.Message.Entities[1].User is not null)
            {
                user = update.Message.Entities[1].User;
            }
            // Second entity must be a mention
            else
                return ResolveMentionedUserResult.UserNotMentioned;
        }
        // If didn't mention user in message => look into replied message
        else if (update.Message.ReplyToMessage?.From is not null)
        {
            user = update.Message.ReplyToMessage?.From;
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
        if (user.Id == Bot.User.Id)
        {
            return ResolveMentionedUserResult.BotSelfMention;
        }

        // Mentioned other bot
        if (user.IsBot)
        {
            return ResolveMentionedUserResult.BotMention;
        }

        return new UserDTO()
        {
            Id = user.Id,
            Name = user.GetFullName(),
            Username = user.Username,
        };
    }

    public async Task<bool> IsAdmin(ITelegramBotClient client, long chatId, long userId, CancellationToken cancellationToken)
    {
        var status = (await client.GetChatMemberAsync(chatId, userId, cancellationToken: cancellationToken)).Status;

        return status == ChatMemberStatus.Creator || status == ChatMemberStatus.Administrator;
    }
}
