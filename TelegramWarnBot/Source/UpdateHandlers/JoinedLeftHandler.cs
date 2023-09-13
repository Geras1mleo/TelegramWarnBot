namespace TelegramWarnBot;

public class JoinedLeftHandler : Pipe<UpdateContext>
{
    private readonly ICachedDataContext cachedDataContext;
    private readonly IChatHelper chatHelper;
    private readonly IConfigurationContext configurationContext;
    private readonly IInMemoryCachedDataContext inMemoryCachedDataContext;
    private readonly ILogger<JoinedLeftHandler> logger;
    private readonly IResponseHelper responseHelper;

    public JoinedLeftHandler(Func<UpdateContext, Task> next,
                             IConfigurationContext configurationContext,
                             ICachedDataContext cachedDataContext,
                             IInMemoryCachedDataContext inMemoryCachedDataContext,
                             IChatHelper chatHelper,
                             IResponseHelper responseHelper,
                             ILogger<JoinedLeftHandler> logger) : base(next)
    {
        this.configurationContext = configurationContext;
        this.cachedDataContext = cachedDataContext;
        this.inMemoryCachedDataContext = inMemoryCachedDataContext;
        this.chatHelper = chatHelper;
        this.responseHelper = responseHelper;
        this.logger = logger;
    }

    public override async Task<Task> Handle(UpdateContext context)
    {
        if (context.Update.Message!.Type == MessageType.ChatMembersAdded)
        {
            // If bot self has been added to new chat => greeting message
            if (context.Update.Message.NewChatMembers.Any(m => m.Id == context.Bot.Id))
            {
                context.ChatDTO = cachedDataContext.CacheChat(context.Update.Message.Chat,
                                                              await chatHelper.GetAdminsAsync(context.Update.Message.Chat.Id,
                                                                                              context.Bot.Id,
                                                                                              context.CancellationToken));

                await responseHelper.SendMessageAsync(new ResponseContext
                {
                    Message = configurationContext.Configuration.Captions.OnBotJoinedChatMessage
                }, context);

                logger.LogWarning("Bot has been added to chat {chat} by user {user}", $"{context.ChatDTO.Name}: {context.ChatDTO.Id}", context.UserDTO);

                return Task.CompletedTask;
            }

            return HandleJoinedAsync(context);
        }
        if (context.Update.Message.Type == MessageType.ChatMemberLeft)
        {
            // If bot left chat / kicked from chat => clear data
            if (context.Update.Message.LeftChatMember!.Id == context.Bot.Id)
            {
                //cachedDataContext.Warnings.RemoveAll(w => w.ChatId == context.ChatDTO.Id);
                //cachedDataContext.Chats.RemoveAll(c => c.Id == context.ChatDTO.Id);

                logger.LogWarning("Bot has been kicked from chat {chat} by user {user}",
                                  $"{context.Update.Message.Chat.Title}: {context.Update.Message.Chat.Id}", context.UserDTO);

                return Task.CompletedTask;
            }

            return HandleLeftAsync(context);
        }

        return next(context);
    }

    private async Task HandleJoinedAsync(UpdateContext context)
    {
        if (!context.IsChatRegistered)
            return;

        if (configurationContext.Configuration.DeleteJoinedLeftMessage)
            if (context.IsBotAdmin)
            {
                await responseHelper.DeleteMessageAsync(context);

                logger.LogInformation("Deleted joined message in chat {chat} successfully!", context.ChatDTO.Name);
            }

        foreach (var member in context.Update.Message.NewChatMembers)
            if (!member.IsBot)
            {
                context.UserDTO = cachedDataContext.CacheUser(member);
                inMemoryCachedDataContext.Members.Add(new MemberDTO
                {
                    ChatId = context.Update.Message.Chat.Id,
                    UserId = member.Id,
                    JoinedDate = DateTime.Now
                });
            }
    }

    private async Task HandleLeftAsync(UpdateContext context)
    {
        if (!context.IsChatRegistered)
            return;

        if (configurationContext.Configuration.DeleteJoinedLeftMessage)
            if (context.IsBotAdmin)
            {
                await responseHelper.DeleteMessageAsync(context);

                logger.LogInformation("Deleted left message in chat {chat} successfully!", context.ChatDTO.Name);
            }
    }
}