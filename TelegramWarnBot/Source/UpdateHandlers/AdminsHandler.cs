namespace TelegramWarnBot;

[RegisteredChat]
public class AdminsHandler : Pipe<UpdateContext>
{
    private readonly IChatHelper chatHelper;

    public AdminsHandler(Func<UpdateContext, Task> next,
                         IChatHelper chatHelper) : base(next)
    {
        this.chatHelper = chatHelper;
    }

    public override Task Handle(UpdateContext context)
    {
        if (context.Update.Type == UpdateType.ChatMember)
        {
            return ChatMemberRightsChanged(context);
        }
        else if (context.Update.Type == UpdateType.MyChatMember)
        {
            return BotRightsChanged(context);
        }

        return next(context);
    }

    private Task ChatMemberRightsChanged(UpdateContext context)
    {
        if (context.Update.ChatMember.NewChatMember.Status == ChatMemberStatus.Administrator)
        {
            context.ChatDTO?.Admins.Add(context.Update.ChatMember.NewChatMember.User.Id);
        }
        else
        {
            context.ChatDTO?.Admins.Remove(context.Update.ChatMember.NewChatMember.User.Id);
        }

        return Task.CompletedTask;
    }

    private async Task BotRightsChanged(UpdateContext context)
    {
        if (context.Update.MyChatMember.NewChatMember.Status == ChatMemberStatus.Administrator)
        {
            if (context.Update.MyChatMember.NewChatMember is ChatMemberAdministrator administrator)
            {
                if (administrator.CanDeleteMessages && administrator.CanRestrictMembers)
                {
                    context.ChatDTO.Admins = await chatHelper.GetAdminsAsync(context.Client, context.ChatDTO.Id, context.CancellationToken);
                    return;
                }
            }
        }

        context.ChatDTO?.Admins.Remove(context.Update.MyChatMember.NewChatMember.User.Id);
    }
}
