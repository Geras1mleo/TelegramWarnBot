namespace TelegramWarnBot;

public interface IUpdateContextBuilder
{
    UpdateContext Build(ITelegramBotClient client, Update update, User botUser, CancellationToken cancellationToken);
}

public class UpdateContextBuilder : IUpdateContextBuilder
{
    private readonly ICachedDataContext cachedDataContext;
    private readonly IChatHelper chatHelper;

    public UpdateContextBuilder(ICachedDataContext cachedDataContext,
                                IChatHelper chatHelper)
    {
        this.cachedDataContext = cachedDataContext;
        this.chatHelper = chatHelper;
    }

    public UpdateContext Build(ITelegramBotClient client, Update update, User botUser, CancellationToken cancellationToken)
    {
        var chatId = update.GetChat().Id;

        var chatDto = cachedDataContext.Chats.Find(c => c.Id == chatId);

        var fromUser = update.GetFromUser();

        var userDto = cachedDataContext.Users.Find(u => u.Id == fromUser.Id);

        return new UpdateContext
        {
            Client = client,
            Update = update,
            CancellationToken = cancellationToken,
            Bot = botUser,
            ChatDTO = chatDto,
            UserDTO = userDto,
            IsMessageUpdate = update.Type == UpdateType.Message,
            IsText = update.Message?.Text is not null,
            IsJoinedLeftUpdate = update.Type == UpdateType.Message &&
                                    (update.Message.Type == MessageType.ChatMembersAdded
                                  || update.Message.Type == MessageType.ChatMemberLeft),
            IsAdminsUpdate = (update.Type == UpdateType.ChatMember
                            || update.Type == UpdateType.MyChatMember)
                          && (update.GetOldMember().Status == ChatMemberStatus.Administrator
                            || update.GetNewMember().Status == ChatMemberStatus.Administrator),
            IsChatRegistered = chatHelper.IsChatRegistered(chatId),
            IsBotAdmin = chatDto?.Admins.Any(a => a == botUser.Id) ?? false,
            IsSenderAdmin = chatDto?.Admins.Any(a => a == fromUser.Id) ?? false,
        };
    }
}
