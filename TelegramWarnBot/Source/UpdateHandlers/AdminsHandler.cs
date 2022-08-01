namespace TelegramWarnBot;

[RegisteredChat]
public class AdminsHandler : Pipe<UpdateContext>
{
    private readonly IChatHelper chatHelper;
    private readonly ILogger<AdminsHandler> logger;

    public AdminsHandler(Func<UpdateContext, Task> next,
                         IChatHelper chatHelper,
                         ILogger<AdminsHandler> logger) : base(next)
    {
        this.chatHelper = chatHelper;
        this.logger = logger;
    }

    public override async Task<Task> Handle(UpdateContext context)
    {
        if (context.Update.Type == UpdateType.ChatMember)
        {
            return ChatMemberRightsChanged(context);
        }
        else if (context.Update.Type == UpdateType.MyChatMember)
        {
            var isAdmin = await BotRightsChanged(context);

            logger.LogInformation("Bot rights in chat {chat} have been updated. Bot is admin: {isAdmin}",
                                  context.ChatDTO.Name, isAdmin);

            return Task.CompletedTask;
        }

        return next(context);
    }

    private Task ChatMemberRightsChanged(UpdateContext context)
    {
        if (context.Update.ChatMember.NewChatMember.Status == ChatMemberStatus.Administrator)
        {
            context.ChatDTO.Admins.Add(context.Update.ChatMember.NewChatMember.User.Id);
        }
        else
        {
            context.ChatDTO.Admins.Remove(context.Update.ChatMember.NewChatMember.User.Id);
        }

        return Task.CompletedTask;
    }

    private async Task<bool> BotRightsChanged(UpdateContext context)
    {
        if (context.Update.MyChatMember.NewChatMember.Status == ChatMemberStatus.Administrator)
        {
            if (context.Update.MyChatMember.NewChatMember is ChatMemberAdministrator administrator)
            {
                if (administrator.CanDeleteMessages && administrator.CanRestrictMembers)
                {
                    context.ChatDTO.Admins = await chatHelper.GetAdminsAsync(context.ChatDTO.Id,
                                                                             context.CancellationToken);
                    return true;
                }
            }
        }

        context.ChatDTO.Admins.Remove(context.Update.MyChatMember.NewChatMember.User.Id);

        return false;
    }
}
