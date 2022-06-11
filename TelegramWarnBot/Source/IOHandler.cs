namespace TelegramWarnBot;

public static class IOHandler
{
    private static readonly string ExecutablePath = AppDomain.CurrentDomain.BaseDirectory;

    private static Configuration configuration;
    private static Trigger[] triggers;
    private static IllegalTrigger[] illegalTriggers;

    private static List<ChatWarnings> warnings;
    private static List<UserDTO> users;
    private static List<ChatDTO> chats;

    static IOHandler()
    {
        GetConfiguration();
        GetTriggers();
        GetIllegalTriggers();
        GetWarnings();
        GetUsers();
        GetChats();
    }

    public static BotConfiguration GetBotConfiguration()
    {
        return Deserialize<BotConfiguration>("Bot.json");
    }

    public static Configuration GetConfiguration()
    {
        if (configuration is null)
            configuration = Deserialize<Configuration>(Path.Combine("Configuration", "Configuration.json"));

        return configuration;
    }

    public static Trigger[] GetTriggers()
    {
        if (triggers is null)
            triggers = Deserialize<Trigger[]>(Path.Combine("Configuration", "Triggers.json"));

        return triggers;
    }

    public static IllegalTrigger[] GetIllegalTriggers()
    {
        if (illegalTriggers is null)
            illegalTriggers = Deserialize<IllegalTrigger[]>(Path.Combine("Configuration", "IllegalTriggers.json"));

        return illegalTriggers;
    }

    public static void ReloadConfiguration()
    {
        configuration = null;
        triggers = null;
        illegalTriggers = null;
        GetConfiguration();
        GetTriggers();
        GetIllegalTriggers();
    }

    public static List<ChatWarnings> GetWarnings()
    {
        if (warnings is null)
            warnings = Deserialize<List<ChatWarnings>>(Path.Combine("Data", "ChatWarnings.json"));

        return warnings;
    }

    private static Task SaveWarningsAsync()
    {
        return SerializeAsync(warnings, Path.Combine("Data", "ChatWarnings.json"));
    }

    public static List<UserDTO> GetUsers()
    {
        if (users is null)
            users = Deserialize<List<UserDTO>>(Path.Combine("Data", "Users.json"));

        return users;
    }

    private static Task SaveUsersAsync()
    {
        return SerializeAsync(users, Path.Combine("Data", "Users.json"));
    }

    public static List<ChatDTO> GetChats()
    {
        if (chats is null)
            chats = Deserialize<List<ChatDTO>>(Path.Combine("Data", "Chats.json"));

        return chats;
    }

    private static Task SaveChatsAsync()
    {
        return SerializeAsync(chats, Path.Combine("Data", "Chats.json"));
    }

    public static void RegisterUser(long id, string username, string name)
    {
        // Adding client to list if not exist
        var user = users.FirstOrDefault(u => u.Id == id);
        if (user is null)
        {
            user = new()
            {
                Id = id,
                Username = username?.ToLower(),
                Name = name,
            };
            users.Add(user);
        }
    }

    public static void RegisterChat(long id, string name)
    {
        // Adding chat to list if not exist
        var chat = chats.FirstOrDefault(c => c.Id == id);
        if (chat is null)
        {
            chat = new()
            {
                Id = id,
                Name = name,
            };
            chats.Add(chat);
        }
    }

    public static Task LogErrorAsync(BotError error)
    {
        var errors = Deserialize<List<BotError>>(Path.Combine("Data", "Logs.json"));
        errors.Add(error);
        return SerializeAsync(errors, Path.Combine("Data", "Logs.json"));
    }

    public static void BeginUpdate(int delaySeconds, CancellationToken cancellationToken)
    {
        Task.Run(async () =>
        {
            while (true)
            {
                await Task.Delay(delaySeconds * 1000, cancellationToken);
                await SaveUsersAsync();
                await SaveWarningsAsync();
                await SaveChatsAsync();
            }
        }, cancellationToken);
    }

    public static void SaveData()
    {
        Task.WaitAll(SaveUsersAsync(), SaveWarningsAsync(), SaveChatsAsync());
        Tools.WriteColor("[Data saved successfully!]", ConsoleColor.Green, true);
    }

    private static T Deserialize<T>(string path)
    {
        var text = System.IO.File.ReadAllText(Path.Combine(ExecutablePath, path));
        return JsonConvert.DeserializeObject<T>(text) ?? throw new Exception($"U fucker changed {path} file...");
    }

    private static Task SerializeAsync(object value, string path)
    {
        var text = JsonConvert.SerializeObject(value, Formatting.Indented);
        return System.IO.File.WriteAllTextAsync(Path.Combine(ExecutablePath, path), text, Encoding.UTF8);
    }
}
