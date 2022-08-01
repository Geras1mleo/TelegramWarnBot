namespace TelegramWarnBot;

public interface IResponseHelper
{
    string FormatResponseVariables(ResponseContext responseContext, UpdateContext updateContext);
    Task DeleteMessageAsync(UpdateContext context);
    Task SendMessageAsync(ResponseContext responseContext, UpdateContext updateContext, int? replyToMessageId = null);
}

public class ResponseHelper : IResponseHelper
{
    private readonly ITelegramBotClientProvider telegramBotClientProvider;
    private readonly IConfigurationContext configurationContext;
    private readonly ICachedDataContext cachedDataContext;
    private readonly ISmartFormatterProvider formatterProvider;

    public ResponseHelper(ITelegramBotClientProvider telegramBotClientProvider,
                          IConfigurationContext configurationContext,
                          ICachedDataContext cachedDataContext,
                          ISmartFormatterProvider formatterProvider)
    {
        this.telegramBotClientProvider = telegramBotClientProvider;
        this.configurationContext = configurationContext;
        this.cachedDataContext = cachedDataContext;
        this.formatterProvider = formatterProvider;
    }

    public Task SendMessageAsync(ResponseContext responseContext, UpdateContext updateContext, int? replyToMessageId = null)
    {
        return telegramBotClientProvider.Client.SendTextMessageAsync(updateContext.ChatDTO.Id,
                                                                     FormatResponseVariables(responseContext, updateContext),
                                                                     replyToMessageId: replyToMessageId,
                                                                     parseMode: ParseMode.Markdown,
                                                                     cancellationToken: updateContext.CancellationToken);
    }

    public Task DeleteMessageAsync(UpdateContext context)
    {
        return telegramBotClientProvider.Client.DeleteMessageAsync(context.ChatDTO.Id,
                                                                   context.Update.Message.MessageId,
                                                                   context.CancellationToken);
    }

    public string FormatResponseVariables(ResponseContext responseContext, UpdateContext updateContext)
    {
        var arguments = new
        {
            SenderUser = GetUserObject(updateContext.ChatDTO.Id, updateContext.UserDTO?.Id),
            MentionedUser = GetUserObject(updateContext.ChatDTO.Id, responseContext.MentionedUserId),
            configurationContext.Configuration
        };

        return formatterProvider.Formatter.Format(responseContext.Message, arguments);
    }

    private MentionedUserDTO GetUserObject(long chatId, long? userId)
    {
        if (userId is null)
            return null;

        UserDTO user = cachedDataContext.Users.Find(u => u.Id == userId);

        if (user is null)
            return null;

        WarnedUser warnedUser = cachedDataContext.Warnings.Find(c => c.ChatId == chatId)?
                                                 .WarnedUsers.Find(u => u.Id == userId);

        return new()
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Username = user.Username,
            Warnings = warnedUser?.Warnings
        };
    }
}
