namespace TelegramWarnBot;

public class InfoCommand : CommandLineApplication, ICommand
{
    private readonly ICachedDataContext cachedDataContext;

    public InfoCommand(ICachedDataContext cachedDataContext)
    {
        this.cachedDataContext = cachedDataContext;

        Name = "info";
        Description = "Show info about cached chats and users";
    }

    // todo with options

    public int OnExecute()
    {
        if (cachedDataContext.Chats.Count > 0)
        {
            Tools.WriteColor($"\nCached Chats: [{cachedDataContext.Chats.Count}]", ConsoleColor.DarkYellow, true);

            string userName;

            foreach (var chat in cachedDataContext.Chats)
            {
                Tools.WriteColor($"\t[{chat.Name}]", ConsoleColor.DarkMagenta, false);

                Tools.WriteColor($"\tAdmins: [{chat.Admins.Count}]", ConsoleColor.DarkYellow, false);

                foreach (var admin in chat.Admins)
                {
                    userName = cachedDataContext.Users.Find(u => u.Id == admin)?.GetName() ?? $"Not found - {admin}";

                    Tools.WriteColor($"\t\t[{userName}]", ConsoleColor.DarkMagenta, false);
                }
                Console.WriteLine();
            }
        }

        Tools.WriteColor($"\nCached Users: [{cachedDataContext.Users.Count}]", ConsoleColor.DarkYellow, false);

        //if (IOHandler.Users.Count > 0)
        //{
        //    foreach (var user in IOHandler.Users)
        //    {
        //        WriteColor($"\t[{user.Name}]", ConsoleColor.DarkMagenta, false);
        //    }
        //}

        if (cachedDataContext.Warnings.Count > 0)
        {
            Tools.WriteColor($"\nWarnings: [{cachedDataContext.Warnings.SelectMany(w => w.WarnedUsers).Select(u => u.Warnings).Sum()}]", ConsoleColor.DarkYellow, false);

            string chatName, userName;

            foreach (var warning in cachedDataContext.Warnings)
            {
                chatName = cachedDataContext.Chats.Find(c => c.Id == warning.ChatId)?.Name ?? $"Not found - {warning.ChatId}";

                Tools.WriteColor($"\t[{chatName}]:", ConsoleColor.DarkMagenta, false);

                foreach (var user in warning.WarnedUsers)
                {
                    userName = cachedDataContext.Users.Find(u => u.Id == user.Id)?.GetName() ?? $"Not found - {user.Id}";

                    Tools.WriteColor($"\t\t[{userName}] - [{user.Warnings}]", ConsoleColor.DarkMagenta, false);
                }
                Console.WriteLine();
            }
        }

        return 1;
    }
}
