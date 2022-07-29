namespace TelegramWarnBot;

public class RegisterCommand : CommandLineApplication, ICommand
{
    private readonly IConfigurationContext configurationContext;
    private readonly ICachedDataContext cachedDataContext;

    private readonly CommandOption listOption;
    private readonly CommandOption removeOption;
    private readonly CommandArgument chatArgument;

    public RegisterCommand(IConfigurationContext configurationContext,
                           ICachedDataContext cachedDataContext)
    {
        this.configurationContext = configurationContext;
        this.cachedDataContext = cachedDataContext;

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
                Tools.WriteColor("[Chat registered successfully]", ConsoleColor.Green, true);
            }
            else
            {
                if (configurationContext.BotConfiguration.RegisteredChats.Remove(chatId))
                {
                    Tools.WriteColor("[Chat removed successfully]", ConsoleColor.Green, true);
                }
                else
                {
                    Tools.WriteColor("[Chat not found...]", ConsoleColor.Red, true);
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
