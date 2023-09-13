namespace TelegramWarnBot;

[TextMessageUpdate]
public class CommandHandler : Pipe<UpdateContext>
{
    private readonly IConfigurationContext configurationContext;
    private readonly ILogger<CommandHandler> logger;
    private readonly IResponseHelper responseHelper;
    private readonly ICommandsController warnController;

    public CommandHandler(Func<UpdateContext, Task> next,
                          IConfigurationContext configurationContext,
                          ICommandsController warnController,
                          IResponseHelper responseHelper,
                          ILogger<CommandHandler> logger) : base(next)
    {
        this.configurationContext = configurationContext;
        this.warnController = warnController;
        this.responseHelper = responseHelper;
        this.logger = logger;
    }

    public override async Task<Task> Handle(UpdateContext context)
    {
        var command = context.Text.Split(' ', '\n')[0][1..].ToLower();

        var method = Tools.ResolveMethod(warnController.GetType(), command);

        if (method is not null)
        {
            if (!context.IsChatRegistered)
            {
                logger.LogWarning("Attempt to use command {command} in unregistered chat {chat}",
                                  command, $"{context.ChatDTO.Name}: {context.ChatDTO.Id}");

                return responseHelper.SendMessageAsync(new ResponseContext
                {
                    Message = configurationContext.Configuration.Captions.ChatNotRegistered
                }, context);
            }

            await (Task<Task>)method.Invoke(warnController, new object[] { context });
        }

        return next(context);
    }
}