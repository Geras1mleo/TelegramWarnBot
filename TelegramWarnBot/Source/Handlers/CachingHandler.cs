namespace TelegramWarnBot;

public class CachingHandler : Pipe<TelegramUpdateContext>
{
    private readonly CachedDataContext cachedDataContext;

    public CachingHandler(Func<TelegramUpdateContext, Task> next,
                          CachedDataContext cachedDataContext) : base(next)
    {
        this.cachedDataContext = cachedDataContext;
    }

    public override Task Handle(TelegramUpdateContext context)
    {
        cachedDataContext.CacheUser(context.Update.Message.From);

        return next(context);
    }
}
