namespace TelegramWarnBot;

public class CachingHandler : Pipe<UpdateContext>
{
    private readonly ICachedDataContext cachedDataContext;

    public CachingHandler(Func<UpdateContext, Task> next,
                          ICachedDataContext cachedDataContext) : base(next)
    {
        this.cachedDataContext = cachedDataContext;
    }

    public override Task Handle(UpdateContext context)
    {
        // To update nickname (etc.) of each user
        cachedDataContext.CacheUser(context.Update.Message.From);

        return next(context);
    }
}
