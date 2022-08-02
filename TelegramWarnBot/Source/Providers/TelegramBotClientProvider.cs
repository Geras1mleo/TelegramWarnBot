namespace TelegramWarnBot;

public interface ITelegramBotClientProvider
{
    Task<Message> SendMessageAsync(ChatId chatId, string text, int? replyToMessageId = null, CancellationToken cancellationToken = default);
    Task DeleteMessageAsync(ChatId chatId, int messageId, CancellationToken cancellationToken = default);
    Task BanChatMemberAsync(ChatId chatId, long userId, CancellationToken cancellationToken = default);
    Task UnbanChatMemberAsync(ChatId chatId, long userId, CancellationToken cancellationToken = default);
    Task<ChatMember[]> GetChatAdministratorsAsync(ChatId chatId, CancellationToken cancellationToken = default);
    Task<Message> ForwardMessageAsync(ChatId chatId, ChatId fromChatId, int messageId, CancellationToken cancellationToken = default);
    Task<User> GetMeAsync(CancellationToken cancellationToken = default);
    Task LeaveChatAsync(ChatId chatId, CancellationToken cancellationToken = default);
    void StartReceiving(
        Func<ITelegramBotClient, Update, CancellationToken, Task> updateHandler,
        Func<ITelegramBotClient, Exception, CancellationToken, Task> pollingErrorHandler,
        CancellationToken cancellationToken = default);
}

public class TelegramBotClientProvider : ITelegramBotClientProvider
{
    private readonly TelegramBotClient client;

    public static TelegramBotClientProvider Shared { get; private set; } // Only for TelegramSink logging

    public TelegramBotClientProvider(IConfigurationContext configurationContext)
    {
        client = new TelegramBotClient(configurationContext.BotConfiguration.Token);
        Shared = this;
    }

    public Task<Message> SendMessageAsync(ChatId chatId, string text, int? replyToMessageId = null, CancellationToken cancellationToken = default)
    {
        return client.SendTextMessageAsync(chatId, text, ParseMode.Markdown, replyToMessageId: replyToMessageId, cancellationToken: cancellationToken);
    }

    public Task DeleteMessageAsync(ChatId chatId, int messageId, CancellationToken cancellationToken = default)
    {
        return client.DeleteMessageAsync(chatId, messageId, cancellationToken: cancellationToken);
    }

    public Task BanChatMemberAsync(ChatId chatId, long userId, CancellationToken cancellationToken = default)
    {
        return client.BanChatMemberAsync(chatId, userId, cancellationToken: cancellationToken);
    }

    public Task UnbanChatMemberAsync(ChatId chatId, long userId, CancellationToken cancellationToken = default)
    {
        return client.UnbanChatMemberAsync(chatId, userId, onlyIfBanned: true, cancellationToken: cancellationToken);
    }

    public Task<ChatMember[]> GetChatAdministratorsAsync(ChatId chatId, CancellationToken cancellationToken = default)
    {
        return client.GetChatAdministratorsAsync(chatId, cancellationToken: cancellationToken);
    }

    public Task<Message> ForwardMessageAsync(ChatId chatId, ChatId fromChatId, int messageId, CancellationToken cancellationToken = default)
    {
        return client.ForwardMessageAsync(chatId, fromChatId, messageId, cancellationToken: cancellationToken);
    }

    public Task<User> GetMeAsync(CancellationToken cancellationToken = default)
    {
        return client.GetMeAsync(cancellationToken: cancellationToken);
    }

    public void StartReceiving(
        Func<ITelegramBotClient, Update, CancellationToken, Task> updateHandler,
        Func<ITelegramBotClient, Exception, CancellationToken, Task> pollingErrorHandler,
        CancellationToken cancellationToken = default)
    {
        client.StartReceiving(updateHandler, pollingErrorHandler,
        receiverOptions: new ReceiverOptions()
        {
            AllowedUpdates = new[]
            {
                UpdateType.Message,
                UpdateType.ChatMember,
                UpdateType.MyChatMember
            },
        }, cancellationToken: cancellationToken);
    }

    public Task LeaveChatAsync(ChatId chatId, CancellationToken cancellationToken = default)
    {
        return client.LeaveChatAsync(chatId, cancellationToken: cancellationToken);
    }
}