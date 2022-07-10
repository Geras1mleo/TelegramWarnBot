namespace TelegramWarnBot;

public class JoinedLeftHandler : Pipe<TelegramUpdateContext>
{
    private readonly ConfigurationContext configurationContext;
    private readonly CachedDataContext cachedDataContext;
    private readonly ChatService chatService;

    public JoinedLeftHandler(Func<TelegramUpdateContext, Task> next,
                             ConfigurationContext configurationContext,
                             CachedDataContext cachedDataContext,
                             ChatService chatService) : base(next)
    {
        this.configurationContext = configurationContext;
        this.cachedDataContext = cachedDataContext;
        this.chatService = chatService;
    }

    public override async Task<Task> Handle(TelegramUpdateContext context)
    {
        if (context.Update.Message.Type == MessageType.ChatMembersAdded)
        {
            // If bot self has been added to new chat => greeting message
            if (context.Update.Message.NewChatMembers.Any(m => m.Id == context.Bot.Id))
            {
                cachedDataContext.CacheChat(context.Update.Message.Chat,
                                           await chatService.GetAdminsAsync(context.Client, context.Update.Message.Chat.Id, context.CancellationToken));

                return context.Client.SendTextMessageAsync(context.Update.Message.Chat.Id,
                          configurationContext.Configuration.Captions.OnBotJoinedChatMessage,
                          cancellationToken: context.CancellationToken, parseMode: ParseMode.Markdown);
            }

            return HandleJoinedAsync(context);
        }
        else if (context.Update.Message.Type == MessageType.ChatMemberLeft)
        {
            // If bot left chat / kicked from chat => clear data
            if (context.Update.Message.LeftChatMember.Id == context.Bot.Id)
            {
                cachedDataContext.Warnings.RemoveAll(w => w.ChatId == context.Update.Message.Chat.Id);
                cachedDataContext.Chats.RemoveAll(c => c.Id == context.Update.Message.Chat.Id);

                return Task.CompletedTask;
            }

            return HandleLeftAsync(context);
        }

        return next(context);
    }

    private Task HandleJoinedAsync(TelegramUpdateContext context)
    {
        if (!context.IsChatRegistered)
            return Task.CompletedTask;

        if (configurationContext.Configuration.DeleteJoinedLeftMessage)
        {
            if (context.IsBotAdmin)
                return context.Client.DeleteMessageAsync(context.Update.Message.Chat.Id,
                                                         context.Update.Message.MessageId,
                                                         context.CancellationToken);
        }

        foreach (var member in context.Update.Message.NewChatMembers)
        {
            if (!member.IsBot)
                cachedDataContext.CacheUser(member);
        }

        return Task.CompletedTask;
    }

    private Task HandleLeftAsync(TelegramUpdateContext context)
    {
        if (!context.IsChatRegistered)
            return Task.CompletedTask;

        else if (configurationContext.Configuration.DeleteJoinedLeftMessage)
        {
            if (context.IsBotAdmin)
                return context.Client.DeleteMessageAsync(context.Update.Message.Chat.Id,
                                                         context.Update.Message.MessageId,
                                                         context.CancellationToken);
        }

        return Task.CompletedTask;
    }
}
