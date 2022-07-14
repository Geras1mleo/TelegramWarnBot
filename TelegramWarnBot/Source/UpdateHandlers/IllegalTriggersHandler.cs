namespace TelegramWarnBot;

[RegisteredChat]
[TextMessageUpdate]
public class IllegalTriggersHandler : Pipe<UpdateContext>
{
    private readonly IConfigurationContext configurationContext;
    private readonly ICachedDataContext cachedDataContext;
    private readonly IMessageHelper messageHelper;
    private readonly IResponseHelper responseHelper;
    private readonly ICommandService commandService;

    public IllegalTriggersHandler(Func<UpdateContext, Task> next,
                                  IConfigurationContext configurationContext,
                                  ICachedDataContext cachedDataContext,
                                  IMessageHelper messageHelper,
                                  IResponseHelper responseHelper,
                                  ICommandService commandService) : base(next)
    {
        this.configurationContext = configurationContext;
        this.cachedDataContext = cachedDataContext;
        this.messageHelper = messageHelper;
        this.responseHelper = responseHelper;
        this.commandService = commandService;
    }

    public override async Task<Task> Handle(UpdateContext context)
    {
        foreach (var trigger in configurationContext.IllegalTriggers)
        {
            // Illegal triggers => ignore admins?
            if (trigger.IgnoreAdmins && context.IsSenderAdmin)
                continue;

            // Applicapble in specific chat
            if (trigger.Chat is not null && trigger.Chat != context.Update.Message.Chat.Id)
                continue;

            if (!messageHelper.MatchMessage(trigger.IllegalWords, false, false, context.Update.Message.Text))
                continue;

            foreach (var adminId in trigger.NotifiedAdmins)
            {
                await context.Client.SendTextMessageAsync(adminId,
                                                  $"*Illegal message detected!*\nChat: *{context.Update.Message.Chat.Title}*" +
                                                  $"\nFrom: *{context.Update.Message.From?.GetFullName()}*" +
                                                  $"\nSent: {context.Update.Message.Date}" +
                                                  $"\nContent:",
                                                  cancellationToken: context.CancellationToken,
                                                  parseMode: ParseMode.Markdown);

                await context.Client.ForwardMessageAsync(adminId, context.Update.Message.Chat.Id,
                                                 context.Update.Message.MessageId,
                                                 cancellationToken: context.CancellationToken);
            }

            // Notify but don't warn admins and dont delete message if not allowed in config
            if (!context.IsSenderAdmin || configurationContext.Configuration.AllowAdminWarnings)
            {
                if (trigger.WarnMember)
                {
                    var chat = commandService.ResolveChatWarning(context.Update.Message.Chat.Id, cachedDataContext.Warnings);
                    var user = commandService.ResolveWarnedUser(context.Update.Message.From.Id, chat);

                    var banned = await commandService.Warn(user, chat.ChatId, null, !context.IsSenderAdmin, context.Client, context.CancellationToken);


                    await context.Client.SendTextMessageAsync(context.Update.Message.Chat.Id,
                                                      responseHelper.ResolveResponseVariables(
                                                                        banned ? configurationContext.Configuration.Captions.IllegalTriggerBanned
                                                                                : configurationContext.Configuration.Captions.IllegalTriggerWarned,
                                                                        user, context.Update.Message.From.GetFullName()),
                                                                    cancellationToken: context.CancellationToken,
                                                                    parseMode: ParseMode.Markdown);
                }

                if (trigger.DeleteMessage)
                {
                    return context.Client.DeleteMessageAsync(context.Update.Message.Chat.Id,
                                                             context.Update.Message.MessageId,
                                                             context.CancellationToken);
                }
            }

            // Match only 1 trigger
            return next(context);
        }

        return next(context);
    }
}
