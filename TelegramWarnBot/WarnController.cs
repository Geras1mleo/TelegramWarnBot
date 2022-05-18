namespace TelegramWarnBot;

public class WarnController
{
    public BotResponse Warn(ITelegramBotClient client, Update update, CancellationToken cancellationToken)
    {
        WarnedUserDTO warnedUser = null;

        var resolve = ResolveWarnedRoot(client, update, cancellationToken);

        warnedUser = resolve.Match(user => user, error => null);

        if (warnedUser is null)
            return new(ResponseType.Error, resolve.AsT1);

        warnedUser.WarnedCount++;

        if (warnedUser.WarnedCount <= IOHandler.GetConfiguration().MaxWarnings)
            return new(ResponseType.Succes, FormatUsers(IOHandler.GetConfiguration().Captions.WarnedSuccessfully, warnedUser));

        client.BanChatMemberAsync(new ChatId(update.Message.Chat.Id), warnedUser.Id, cancellationToken: cancellationToken);
        return new(ResponseType.Succes, FormatUsers(IOHandler.GetConfiguration().Captions.BannedSuccessfully, warnedUser));
    }

    public BotResponse Unwarn(ITelegramBotClient client, Update update, CancellationToken cancellationToken)
    {
        WarnedUserDTO warnedUser = null;

        var resolve = ResolveWarnedRoot(client, update, cancellationToken);

        warnedUser = resolve.Match(user => user, error => null);

        if (warnedUser is null)
            return new(ResponseType.Error, resolve.AsT1);

        if (warnedUser.WarnedCount == 0)
        {
            return new(ResponseType.Error, IOHandler.GetConfiguration().Captions.UserHasNoWarnings);
        }

        warnedUser.WarnedCount--;

        client.DeleteMessageAsync(new ChatId(update.Message.Chat.Id), update.Message.MessageId, cancellationToken);

        client.UnbanChatMemberAsync(new ChatId(update.Message.Chat.Id), warnedUser.Id, onlyIfBanned: true, cancellationToken: cancellationToken);

        return new(ResponseType.Succes, FormatUsers(IOHandler.GetConfiguration().Captions.UnwarnedSuccessfully, warnedUser));
    }

    private static string FormatUsers(string value, WarnedUserDTO user)
    {
        return value.Replace("{warnedUser.WarnedCount}", user.WarnedCount.ToString()).Replace("{warnedUser}", Tools.GetMentionString(user.Name, user.Id));
    }

    private static OneOf<WarnedUserDTO, string> ResolveWarnedRoot(ITelegramBotClient client, Update update, CancellationToken cancellationToken)
    {
        if (!CheckPermissions(client, update.Message.Chat.Id, update.Message.From.Id, cancellationToken))
            return IOHandler.GetConfiguration().Captions.NoPermissions;

        if (!CheckPermissions(client, update.Message.Chat.Id, BotHandler.MeUser.Id, cancellationToken))
            return IOHandler.GetConfiguration().Captions.BotHasNoPermissions;

        UserDTO user = null;
        var resolve = ResolveMentionedUser(update);

        user = resolve.Match(user => user, error => null);

        if (user is null)
            return resolve.AsT1;

        var warnings = IOHandler.GetWarnings();

        var chat = ResolveChat(update, warnings);

        return ResolveWarnedUser(user, chat);
    }

    private static WarnedUserDTO ResolveWarnedUser(UserDTO user, ChatDTO chat)
    {
        var warnedUser = chat.WarnedUsers.FirstOrDefault(u => u.Id == user.Id);

        if (warnedUser is null)
        {
            warnedUser = new()
            {
                Id = user.Id,
                Username = user.Username,
                Name = user.Name,
                WarnedCount = 0
            };
            chat.WarnedUsers.Add(warnedUser);
        }
        return warnedUser;
    }

    private static ChatDTO ResolveChat(Update update, Warnings warnings)
    {
        var chat = warnings.Chats.FirstOrDefault(c => c.Id == update.Message.Chat.Id);

        if (chat is null)
        {
            chat = new()
            {
                Id = update.Message.Chat.Id,
                Name = update.Message.Chat.Title,
                WarnedUsers = new List<WarnedUserDTO>()
            };
            warnings.Chats.Add(chat);
        }
        return chat;
    }

    private static OneOf<UserDTO, string> ResolveMentionedUser(Update update)
    {
        var allUsers = IOHandler.GetUsers();
        UserDTO user = null;

        if (update.Message.Entities is not null
         && update.Message.Entities.Length == 2)
        {
            if (update.Message.Entities[1].Type == MessageEntityType.Mention
             && update.Message?.EntityValues is not null)
            {
                var mentionedUser = update.Message.EntityValues.ElementAt(1)[1..].ToLower();

                if (mentionedUser == BotHandler.MeUser.Username?.ToLower())
                    return IOHandler.GetConfiguration().Captions.Angry;

                user = allUsers.FirstOrDefault(u => u.Username == mentionedUser);

                if (user is null)
                    return IOHandler.GetConfiguration().Captions.UserNotFound;
            }
            else if (update.Message.Entities[1].Type == MessageEntityType.TextMention
                  && update.Message.Entities[1].User is not null)
            {
                user = new()
                {
                    Id = update.Message.Entities[1].User.Id,
                    Username = update.Message.Entities[1].User.Username,
                    Name = update.Message.Entities[1].User.FirstName,
                };
            }
        }

        // If didn't mention user in message => look to replied message
        if (user is null)
        {
            if (update.Message.ReplyToMessage?.From is null)
                return IOHandler.GetConfiguration().Captions.UserNotSpecified;

            if (update.Message.ReplyToMessage.From.Id == BotHandler.MeUser.Id)
                return IOHandler.GetConfiguration().Captions.Angry;

            if (update.Message.ReplyToMessage.From.IsBot)
                return IOHandler.GetConfiguration().Captions.BotWarnAttempt;

            user = new()
            {
                Id = update.Message.ReplyToMessage.From.Id,
                Username = update.Message.ReplyToMessage.From.Username,
                Name = update.Message.ReplyToMessage.From.FirstName,
            };

            if (user is null)
                return IOHandler.GetConfiguration().Captions.UserNotFound;
        }
        return user;
    }

    private static bool CheckPermissions(ITelegramBotClient client, long chatId, long userId, CancellationToken cancellationToken)
    {
        var status = client.GetChatMemberAsync(new ChatId(chatId), userId, cancellationToken: cancellationToken)
                           .GetAwaiter()
                           .GetResult()
                           .Status;

        return status == ChatMemberStatus.Creator || status == ChatMemberStatus.Administrator;
    }
}
