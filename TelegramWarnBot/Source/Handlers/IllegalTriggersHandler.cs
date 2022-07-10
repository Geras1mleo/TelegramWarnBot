namespace TelegramWarnBot;

public class IllegalTriggersHandler : Pipe<TelegramUpdateContext>
{
    private readonly ConfigurationContext configurationContext;
    private readonly CachedDataContext cachedDataContext;
    private readonly ChatService chatService;
    private readonly UpdateHelper updateHelper;
    private readonly ResponseHelper responseHelper;

    public IllegalTriggersHandler(Func<TelegramUpdateContext, Task> next,
                                  ConfigurationContext configurationContext,
                                  CachedDataContext cachedDataContext,
                                  ChatService chatService,
                                  UpdateHelper updateHelper,
                                  ResponseHelper responseHelper) : base(next)
    {
        this.configurationContext = configurationContext;
        this.cachedDataContext = cachedDataContext;
        this.chatService = chatService;
        this.updateHelper = updateHelper;
        this.responseHelper = responseHelper;
    }

    public override async Task<Task> Handle(TelegramUpdateContext context)
    {
        foreach (var trigger in configurationContext.IllegalTriggers)
        {
            // Illegal triggers => ignore admins?
            if (trigger.IgnoreAdmins && context.IsSenderAdmin)
                continue;

            // Applicapble in specific chat
            if (trigger.Chat is not null && trigger.Chat != context.Update.Message.Chat.Id)
                continue;

            if (!updateHelper.MatchMessage(trigger.IllegalWords, false, false, context.Update.Message.Text))
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
                    var chat = chatService.ResolveChatWarning(context.Update.Message.Chat.Id, cachedDataContext.Warnings);
                    var user = chatService.ResolveWarnedUser(context.Update.Message.From.Id, chat);

                    var banned = await chatService.Warn(user, chat.ChatId, null, !context.IsSenderAdmin, context.Client, context.CancellationToken);


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
                    await context.Client.DeleteMessageAsync(context.Update.Message.Chat.Id,
                                                            context.Update.Message.MessageId,
                                                            context.CancellationToken);
                }



                // Match only 1 trigger
                return next(context);
            }
        }

        return next(context);
    }
}
