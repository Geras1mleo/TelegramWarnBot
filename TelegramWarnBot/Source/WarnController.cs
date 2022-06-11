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
        var resolve = await service.ResolveWarnedRoot(client, update, true, cancellationToken);

        if (!resolve.TryPickT0(out WarnedUser warnedUser, out _))
            return new(resolve.AsT1);

        var banned = await service.Warn(warnedUser,
                                        update.Message.Chat.Id,
                                        IOHandler.GetConfiguration().DeleteWarnMessage ? update.Message.MessageId : null,
                                        client, cancellationToken);

        // Notify in chat that user has been warned or banned
        return new(Tools.ResolveResponseVariables(banned ? IOHandler.GetConfiguration().Captions.BannedSuccessfully
                                                         : IOHandler.GetConfiguration().Captions.WarnedSuccessfully,
                                                  warnedUser,
                                                  service.ResolveMentionedUser(update).AsT0.Name));
    }

    public async Task<BotResponse> Unwarn(ITelegramBotClient client, Update update, CancellationToken cancellationToken)
    {
        var resolve = await service.ResolveWarnedRoot(client, update, false, cancellationToken);

        if (!resolve.TryPickT0(out WarnedUser warnedUser, out _))
            return new(resolve.AsT1);

        if (warnedUser.Warnings == 0)
        {
            return new(Tools.ResolveResponseVariables(IOHandler.GetConfiguration().Captions.UserHasNoWarnings,
                                                      warnedUser,
                                                      service.ResolveMentionedUser(update).AsT0.Name));
        }

        warnedUser.Warnings--;

        if (IOHandler.GetConfiguration().DeleteWarnMessage)
            await client.DeleteMessageAsync(update.Message.Chat.Id, update.Message.MessageId, cancellationToken);

        await client.UnbanChatMemberAsync(update.Message.Chat.Id, warnedUser.Id, onlyIfBanned: true, cancellationToken: cancellationToken);

        return new(Tools.ResolveResponseVariables(IOHandler.GetConfiguration().Captions.UnwarnedSuccessfully,
                                                  warnedUser,
                                                  service.ResolveMentionedUser(update).AsT0.Name));
    }

    public Task<BotResponse> WCount(ITelegramBotClient client, Update update, CancellationToken cancellationToken)
    {
        var chat = service.ResolveChat(update, IOHandler.GetWarnings());

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
                ResolveMentionedUserResult.UserNotFound => Task.FromResult(new BotResponse(IOHandler.GetConfiguration().Captions.UserNotFound)),
                ResolveMentionedUserResult.BotMention => Task.FromResult(new BotResponse("What do you mean? My bot's are holy")), // todo from config
                ResolveMentionedUserResult.BotSelfMention => Task.FromResult(new BotResponse("I'm holy!")),
                _ => throw new ArgumentException("ResolveMentionedUserResult")
            };
        }

        var count = IOHandler.GetWarnings().Find(c => c.ChatId == chat.ChatId)?.WarnedUsers.Find(u => u.Id == user.Id)?.Warnings ?? 0;

        if (count == 0)
        {
            return Task.FromResult<BotResponse>(new(Tools.ResolveResponseVariables(IOHandler.GetConfiguration().Captions.UserHasNoWarnings, user, 0)));
        }

        return Task.FromResult<BotResponse>(new(Tools.ResolveResponseVariables(IOHandler.GetConfiguration().Captions.WarningsCount, user, count)));
    }
}
