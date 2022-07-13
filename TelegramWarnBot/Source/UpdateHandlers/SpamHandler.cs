namespace TelegramWarnBot;

public class SpamHandler : Pipe<UpdateContext>
{
    private readonly ICachedDataContext cachedDataContext;

    public SpamHandler(Func<UpdateContext, Task> next,
                       ICachedDataContext cachedDataContext) : base(next)
    {
        this.cachedDataContext = cachedDataContext;
    }

    public override Task Handle(UpdateContext context)
    {
        long chatId = context.Update.Message.Chat.Id;

        if (context.Update.Message.Entities?.Any(e => e.Type == MessageEntityType.Url || e.Type == MessageEntityType.TextLink) ?? false)
        {
            var member = cachedDataContext.Members.FirstOrDefault(m => m.ChatId == chatId && m.UserId == context.Update.Message.From.Id);

            if (DateTime.Now - (member?.JoinedDate ?? DateTime.MinValue) < TimeSpan.FromHours(24))
            {
                return context.Client.DeleteMessageAsync(chatId, context.Update.Message.MessageId, context.CancellationToken);
            }
        }

        return next(context);
    }
}
