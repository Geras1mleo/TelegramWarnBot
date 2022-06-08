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
        WarnedUser warnedUser = null;

        var resolve = service.ResolveWarnedRoot(client, update, cancellationToken);

        warnedUser = resolve.Match(user => user, error => null);

        if (warnedUser is null)
            return new(resolve.AsT1);

        warnedUser.Warnings++;

        if (IOHandler.GetConfiguration().DeleteWarnMessage)
            client.DeleteMessageAsync(update.Message.Chat.Id, update.Message.MessageId, cancellationToken);

        // If not reached max warnings 
        if (warnedUser.Warnings <= IOHandler.GetConfiguration().MaxWarnings)
        {
            return new(Tools.ResolveResponseVariables(IOHandler.GetConfiguration().Captions.WarnedSuccessfully, warnedUser));
        }

        // Max warnings reached
        client.BanChatMemberAsync(update.Message.Chat.Id,
                                  warnedUser.Id,
                                  cancellationToken: cancellationToken);

        // Notify in chat that user has been banned
        return new(Tools.ResolveResponseVariables(IOHandler.GetConfiguration().Captions.BannedSuccessfully, warnedUser));
    }

    public BotResponse Unwarn(ITelegramBotClient client, Update update, CancellationToken cancellationToken)
    {
        WarnedUser warnedUser = null;

        var resolve = service.ResolveWarnedRoot(client, update, cancellationToken);

        warnedUser = resolve.Match(user => user, error => null);

        if (warnedUser is null)
            return new(resolve.AsT1);

        if (warnedUser.Warnings == 0)
        {
            return new(IOHandler.GetConfiguration().Captions.UserHasNoWarnings);
        }

        warnedUser.Warnings--;

        if (IOHandler.GetConfiguration().DeleteWarnMessage)
            client.DeleteMessageAsync(update.Message.Chat.Id, update.Message.MessageId, cancellationToken);

        client.UnbanChatMemberAsync(update.Message.Chat.Id, warnedUser.Id, onlyIfBanned: true, cancellationToken: cancellationToken);

        return new(Tools.ResolveResponseVariables(IOHandler.GetConfiguration().Captions.UnwarnedSuccessfully, warnedUser));
    }
}
