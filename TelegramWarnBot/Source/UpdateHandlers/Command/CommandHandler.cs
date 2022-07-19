namespace TelegramWarnBot;

[TextMessageUpdate]
public class CommandHandler : Pipe<UpdateContext>
{
    private readonly IConfigurationContext configurationContext;
    private readonly IWarnController warnController;
    private readonly IResponseHelper responseHelper;

    public CommandHandler(Func<UpdateContext, Task> next,
                          IConfigurationContext configurationContext,
                          IWarnController warnController,
                          IResponseHelper responseHelper) : base(next)
    {
        this.configurationContext = configurationContext;
        this.warnController = warnController;
        this.responseHelper = responseHelper;
    }

    public override async Task<Task> Handle(UpdateContext context)
    {
        var method = Tools.ResolveMethod(warnController.GetType(), context.Update.Message.Text.Split(' ')[0][1..]);

        if (method is not null)
        {
            if (!context.IsChatRegistered)
            {
                return responseHelper.SendMessageAsync(new()
                {
                    Message = configurationContext.Configuration.Captions.ChatNotRegistered
                }, context);
            }

            await (Task<Task>)method.Invoke(warnController, new object[] { context });
        }

        return next(context);
    }
}