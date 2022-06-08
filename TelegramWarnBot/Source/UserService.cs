namespace TelegramWarnBot;

public class UserService
{
    /// <summary>
    /// return user or error message that has to be returned
    /// </summary>
    /// <param name="client"></param>
    /// <param name="update"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public OneOf<WarnedUser, string> ResolveWarnedRoot(ITelegramBotClient client, Update update, CancellationToken cancellationToken)
    {
        if (!CheckPermissions(client, update.Message.Chat.Id, update.Message.From.Id, cancellationToken))
            return IOHandler.GetConfiguration().Captions.NoPermissions;

        if (!CheckPermissions(client, update.Message.Chat.Id, Bot.User.Id, cancellationToken))
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

    public WarnedUser ResolveWarnedUser(UserDTO user, ChatDTO chat)
    {
        var warnedUser = chat.WarnedUsers.FirstOrDefault(u => u.Id == user.Id);

        if (warnedUser is null)
        {
            warnedUser = new()
            {
                Id = user.Id,
                Warnings = 0
            };
            chat.WarnedUsers.Add(warnedUser);
        }
        return warnedUser;
    }

    public ChatDTO ResolveChat(Update update, Warnings warnings)
    {
        var chat = warnings.Chats.FirstOrDefault(c => c.Id == update.Message.Chat.Id);

        if (chat is null)
        {
            chat = new()
            {
                Id = update.Message.Chat.Id,
                Name = update.Message.Chat.Title,
                WarnedUsers = new List<WarnedUser>()
            };
            warnings.Chats.Add(chat);
        }
        return chat;
    }

    /// <summary>
    /// return user or error message that has to be returned
    /// </summary>
    /// <param name="update"></param>
    /// <returns></returns>
    public OneOf<UserDTO, string> ResolveMentionedUser(Update update)
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

                if (mentionedUser == Bot.User.Username?.ToLower())
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

            if (update.Message.ReplyToMessage.From.Id == Bot.User.Id)
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

    public bool CheckPermissions(ITelegramBotClient client, long chatId, long userId, CancellationToken cancellationToken)
    {
        var status = client.GetChatMemberAsync(chatId, userId, cancellationToken: cancellationToken)
                           .GetAwaiter()
                           .GetResult()
                           .Status;

        return status == ChatMemberStatus.Creator || status == ChatMemberStatus.Administrator;
    }
}
