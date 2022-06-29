namespace TelegramWarnBot;

public class WarnController
{
    private readonly UserService service;

    public WarnController(UserService service)
    {
        this.service = service;
    }

    public async Task<BotResponse> Warn(ITelegramBotClient client, Update update, CancellationToken cancellationToken)
    {
        var resolve = service.ResolveWarnedRoot(client, update, true, cancellationToken);

        if (!resolve.TryPickT0(out var warnedUser, out _))
            return new(resolve.AsT1);

        var banned = await service.Warn(warnedUser,
                                        update.Message.Chat.Id,
                                        IOHandler.Configuration.DeleteWarnMessage ? update.Message.MessageId : null,
                                        !service.IsAdmin(update.Message.Chat.Id, warnedUser.Id),
                                        client, cancellationToken);

        // Notify in chat that user has been warned or banned
        return new(Tools.ResolveResponseVariables(banned ? IOHandler.Configuration.Captions.BannedSuccessfully
                                                         : IOHandler.Configuration.Captions.WarnedSuccessfully,
                                                  warnedUser,
                                                  service.ResolveMentionedUser(update).AsT0.Name));
    }

    public async Task<BotResponse> Unwarn(ITelegramBotClient client, Update update, CancellationToken cancellationToken)
    {
        var resolve = service.ResolveWarnedRoot(client, update, false, cancellationToken);

        if (!resolve.TryPickT0(out var warnedUser, out _))
            return new(resolve.AsT1);

        if (warnedUser.Warnings == 0)
        {
            return new(Tools.ResolveResponseVariables(IOHandler.Configuration.Captions.UserUnwarnNoWarnings,
                                                      warnedUser,
                                                      service.ResolveMentionedUser(update).AsT0.Name));
        }

        warnedUser.Warnings--;

        if (IOHandler.Configuration.DeleteWarnMessage)
            await client.DeleteMessageAsync(update.Message.Chat.Id, update.Message.MessageId, cancellationToken);

        await client.UnbanChatMemberAsync(update.Message.Chat.Id, warnedUser.Id, onlyIfBanned: true, cancellationToken: cancellationToken);

        return new(Tools.ResolveResponseVariables(IOHandler.Configuration.Captions.UnwarnedSuccessfully,
                                                  warnedUser,
                                                  service.ResolveMentionedUser(update).AsT0.Name));
    }

    public Task<BotResponse> WCount(ITelegramBotClient client, Update update, CancellationToken cancellationToken)
    {
        var chat = service.ResolveChatWarning(update.Message.Chat.Id, IOHandler.Warnings);

        var resolveUser = service.ResolveMentionedUser(update);

        UserDTO user = resolveUser.Match<UserDTO>(
        userDto => userDto,
        result =>
        {
            return result switch
            {
                ResolveMentionedUserResult.UserNotMentioned =>
                user = new()
                {
                    Id = update.Message.From.Id,
                    Name = update.Message.From.GetFullName(),
                },
                _ => null,
            };
        });

        if (user is null)
        {
            return resolveUser.AsT1 switch
            {
                ResolveMentionedUserResult.UserNotFound => Task.FromResult(new BotResponse(IOHandler.Configuration.Captions.UserNotFound)),
                ResolveMentionedUserResult.BotMention => Task.FromResult(new BotResponse(IOHandler.Configuration.Captions.WarningsCountBotMention)),
                ResolveMentionedUserResult.BotSelfMention => Task.FromResult(new BotResponse(IOHandler.Configuration.Captions.WarningsCountBotSelfMention)),
                _ => throw new ArgumentException("ResolveMentionedUserResult")
            };
        }

        if (!IOHandler.Configuration.AllowAdminWarnings)
        {
            var isAdmin = service.IsAdmin(chat.ChatId, user.Id);
            if (isAdmin)
                return Task.FromResult(new BotResponse(IOHandler.Configuration.Captions.WarningsCountAdminNotAllowed));
        }

        var count = IOHandler.Warnings.Find(c => c.ChatId == chat.ChatId)?.WarnedUsers.Find(u => u.Id == user.Id)?.Warnings ?? 0;

        if (count == 0)
        {
            return Task.FromResult(new BotResponse(Tools.ResolveResponseVariables(IOHandler.Configuration.Captions.WarningsCountUserHasNoWarnings, user, 0)));
        }

        return Task.FromResult(new BotResponse(Tools.ResolveResponseVariables(IOHandler.Configuration.Captions.WarningsCount, user, count)));
    }

    public Task<BotResponse> Update(ITelegramBotClient client, Update update, CancellationToken cancellationToken)
    {
        var chat = IOHandler.Chats.Find(c => c.Id == update.Message.Chat.Id);

        if (chat?.Admins is null)
        {
            IOHandler.CacheChat(update.Message.Chat, service.GetAdmins(client, update.Message.Chat.Id, cancellationToken));
        }
        else
        {
            chat.Admins = service.GetAdmins(client, update.Message.Chat.Id, cancellationToken);
        }

        return Task.FromResult(new BotResponse("Admins updated successfully!"));
    }
}
