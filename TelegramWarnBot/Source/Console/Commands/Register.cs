namespace TelegramWarnBot;

public class RegisterCommand : CommandLineApplication, ICommand
{
    private readonly IConfigurationContext configurationContext;
    private readonly ICachedDataContext cachedDataContext;
    private readonly IChatHelper chatHelper;
    private readonly ILogger<RegisterCommand> logger;
    private readonly IBot bot;
    private readonly CommandOption listOption;
    private readonly CommandOption removeOption;
    private readonly CommandArgument chatArgument;

    public RegisterCommand(IConfigurationContext configurationContext,
                           ICachedDataContext cachedDataContext,
                           IChatHelper chatHelper,
                           ILogger<RegisterCommand> logger,
                           IBot bot)
    {
        this.configurationContext = configurationContext;
        this.cachedDataContext = cachedDataContext;
        this.chatHelper = chatHelper;
        this.logger = logger;
        this.bot = bot;
        Name = "register";
        Description = "Manipulate registered chats";

        listOption = Option("-l | --list", "Show the list of registered chats", CommandOptionType.NoValue);
        removeOption = Option("-r | --remove", "Remove one specific chat from list", CommandOptionType.NoValue);
        chatArgument = Argument("Chat Id", "Chat to (add to / remove from) the list",
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

                var chat = cachedDataContext.FindChatById(chatId);

                logger.LogInformation("Chat {chat} registered successfully!",
                                      $"{chat?.Name}: {chatId}");
                if (chat is not null)
                {
                    try
                    {
                        // Admins list hasn't been updated if chat wasn't registered
                        chat.Admins = chatHelper.GetAdminsAsync(chat.Id, bot.BotUser.Id, CancellationToken.None)
                                                .GetAwaiter().GetResult();
                    }
                    catch (Exception e)
                    {
                        logger.LogInformation("Error while loading admins list..\n{message}", e.Message);
                    }
                }
            }
            else
            {
                var chat = cachedDataContext.FindChatById(chatId);

                if (configurationContext.BotConfiguration.RegisteredChats.Remove(chatId))
                {
                    logger.LogInformation("Chat {chat} removed from registered list successfully!",
                                          $"{chat?.Name}: {chatId}");
                }
                else
                {
                    logger.LogWarning("Chat {chatId} has been not registered yet",
                                      $"{chat?.Name}: {chatId}");
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

                Tools.WriteColor("\t[" + (cachedDataContext.FindChatById(chatId)?.Name ?? "Chat not cached yet") + "]: " + chatId,
                                 ConsoleColor.Blue, false);
            }

            var notRegisteredChats = cachedDataContext.Chats.Where(cachedChat =>
            {
                return !configurationContext.BotConfiguration.RegisteredChats.Contains(cachedChat.Id);
            });

            if (!notRegisteredChats.Any())
            {
                return 1;
            }

            Console.WriteLine("\nNot registered chats:");

            foreach (var chat in notRegisteredChats)
            {
                Tools.WriteColor("\t[" + chat.Name + "]: " + chat.Id, ConsoleColor.Red, false);
            }

            Console.WriteLine();

            return 1;
        }

        ShowHelp();

        return 0;
    }
}
