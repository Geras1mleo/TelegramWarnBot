namespace TelegramWarnBot;

public class CachingHandler : Pipe<UpdateContext>
{
    private readonly CachedDataContext cachedDataContext;

    public CachingHandler(Func<UpdateContext, Task> next,
                          CachedDataContext cachedDataContext) : base(next)
    {
        this.cachedDataContext = cachedDataContext;
    }

    public override Task Handle(UpdateContext context)
    {
        cachedDataContext.CacheUser(context.Update.Message.From);

        return next(context);
    }
}
