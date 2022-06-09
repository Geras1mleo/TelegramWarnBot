namespace TelegramWarnBot;

public class WarnController
{
    private readonly UserService service;

    public WarnController(UserService service)
    {
        this.service = service;
    }

    public BotResponse Warn(ITelegramBotClient client, Update update, CancellationToken cancellationToken)
    {
        var resolve = service.ResolveWarnedRoot(client, update, cancellationToken);

        if (!resolve.TryPickT0(out WarnedUser warnedUser, out _))
            return new(resolve.AsT1);

        var banned = service.Warn(warnedUser,
                                update.Message.Chat.Id,
                                IOHandler.GetConfiguration().DeleteWarnMessage ? update.Message.MessageId : null,
                                client, cancellationToken);

        // Notify in chat that user has been warned or banned
        return new(Tools.ResolveResponseVariables(banned ? IOHandler.GetConfiguration().Captions.BannedSuccessfully
                                                         : IOHandler.GetConfiguration().Captions.WarnedSuccessfully,
                                                  warnedUser,
                                                  service.ResolveMentionedUser(update).AsT0.Name));
    }

    public BotResponse Unwarn(ITelegramBotClient client, Update update, CancellationToken cancellationToken)
    {
        var resolve = service.ResolveWarnedRoot(client, update, cancellationToken);

        if (!resolve.TryPickT0(out WarnedUser warnedUser, out _))
            return new(resolve.AsT1);

        if (warnedUser.Warnings == 0)
        {
            return new(IOHandler.GetConfiguration().Captions.UserHasNoWarnings);
        }

        warnedUser.Warnings--;

        if (IOHandler.GetConfiguration().DeleteWarnMessage)
            client.DeleteMessageAsync(update.Message.Chat.Id, update.Message.MessageId, cancellationToken);

        client.UnbanChatMemberAsync(update.Message.Chat.Id, warnedUser.Id, onlyIfBanned: true, cancellationToken: cancellationToken);
         
        return new(Tools.ResolveResponseVariables(IOHandler.GetConfiguration().Captions.UnwarnedSuccessfully,
                                                  warnedUser,
                                                  service.ResolveMentionedUser(update).AsT0.Name));
    }
}
