namespace TelegramWarnBot;

public class RegisterCommand : CommandLineApplication, ICommand
{
    private readonly IBot bot;
    private readonly IConfigurationContext configurationContext;
    private readonly ICachedDataContext cachedDataContext;
    private readonly IChatHelper chatHelper;
    private readonly ILogger<RegisterCommand> logger;

    private readonly CommandOption listOption;
    private readonly CommandOption removeOption;
    private readonly CommandArgument chatArgument;

    public RegisterCommand(IBot bot,
                           IConfigurationContext configurationContext,
                           ICachedDataContext cachedDataContext,
                           IChatHelper chatHelper,
                           ILogger<RegisterCommand> logger)
    {
        this.bot = bot;
        this.configurationContext = configurationContext;
        this.cachedDataContext = cachedDataContext;
        this.chatHelper = chatHelper;
        this.logger = logger;

        Name = "register";
        Description = "Manipulate with registered chats";

        listOption = Option("-l | --list", "Show the list of registered chats", CommandOptionType.NoValue);
        removeOption = Option("-r | --remove", "Remove option for a specific chat", CommandOptionType.NoValue);
        chatArgument = Argument("Chat Id", "Chat to add/remove",
            c => c.Accepts().RegularExpression("^\\\"?\\-?\\d+\"?$", "Not valid chat id"));
    }

    public int OnExecute()
    {
        if (chatArgument.HasValue)
        {
            var chatId = long.Parse(chatArgument.Value.Trim('\"'));

            if (!removeOption.HasValue())
            {
                configurationContext.BotConfiguration.RegisteredChats.Add(chatId);

                var chat = cachedDataContext.Chats.Find(c => c.Id == chatId);

                logger.LogInformation("Chat {chat} registered successfully!",
                                      $"{chat?.Name}: {chatId}");

                // Admins list hasn't been updated if chat wasn't registered
                chat.Admins = chatHelper.GetAdminsAsync(bot.Client,
                                                        chat.Id,
                                                        CancellationToken.None)
                                        .GetAwaiter().GetResult();
            }
            else
            {
                if (configurationContext.BotConfiguration.RegisteredChats.Remove(chatId))
                {
                    logger.LogInformation("Chat {chat} removed from registered list successfully!",
                                          $"{cachedDataContext.Chats.Find(c => c.Id == chatId)?.Name}: {chatId}"); //todo with function getchatbyid
                }
                else
                {
                    logger.LogWarning("Chat {chatId} has been not registered yet...",
                                      $"{cachedDataContext.Chats.Find(c => c.Id == chatId)?.Name}: {chatId}");
                }
            }

            cachedDataContext.SaveRegisteredChatsAsync(configurationContext.BotConfiguration.RegisteredChats).GetAwaiter().GetResult();

            return 1;
        }
        else if (listOption.HasValue())
        {
            Console.WriteLine("\nRegistered chats:");

            foreach (var chatId in configurationContext.BotConfiguration.RegisteredChats)
            {

                Tools.WriteColor("\t[" + (cachedDataContext.Chats.Find(c => c.Id == chatId)?.Name ?? "Chat not cached yet") + "]: " + chatId,
                                 ConsoleColor.Blue, false);
            }

            var notRegisteredChats = cachedDataContext.Chats.Where(cached => !configurationContext.BotConfiguration.RegisteredChats.Contains(cached.Id));

            if (!notRegisteredChats.Any())
            {
                return 1;
            }

            Console.WriteLine("\nNot registered chats:");

            foreach (var chat in notRegisteredChats)
            {
                Tools.WriteColor("\t[" + chat.Name + "]: " + chat.Id, ConsoleColor.Red, false);
            }

            return 1;
        }

        ShowHelp();

        return 0;
    }
}
