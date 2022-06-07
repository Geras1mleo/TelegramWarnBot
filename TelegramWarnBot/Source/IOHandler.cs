namespace TelegramWarnBot;

public static class IOHandler
{
    public static string ExecutablePath = AppDomain.CurrentDomain.BaseDirectory;

    private static Warnings warnings;
    private static List<UserDTO> users;
    private static Configuration configuration;
    private static Trigger[] triggers;
    private static IllegalTrigger[] illegalTriggers;

    static IOHandler()
    {
        GetConfiguration();
        GetWarnings();
        GetUsers();
        GetTriggers();
        GetIllegalTriggers();
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

    public static Configuration ReloadConfiguration()
    {
        return configuration = Deserialize<Configuration>(Path.Combine("Data", "Configuration.json"));
    }

    public static Warnings GetWarnings()
    {
        if (warnings is null)
            warnings = Deserialize<Warnings>(Path.Combine("Data", "Chats.json"));

        return warnings;
    }

    private static Task SaveWarningsAsync()
    {
        return Serialize(warnings, Path.Combine("Data", "Chats.json"));
    }

    public static List<UserDTO> GetUsers()
    {
        if (users is null)
            users = Deserialize<List<UserDTO>>(Path.Combine("Data", "Users.json"));

        return users;
    }

    private static Task SaveUsersAsync()
    {
        return Serialize(users, Path.Combine("Data", "Users.json"));
    }

    public static void RegisterClient(long id, string username, string name)
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

    public static void BeginUpdate(int delaySeconds, CancellationToken cancellationToken)
    {
        Task.Run(async () =>
        {
            while (true)
            {
                await Task.Delay(delaySeconds * 1000, cancellationToken);
                await SaveDataAsync();
            }
        }, cancellationToken);
    }

    private static async Task SaveDataAsync()
    {
        await SaveUsersAsync();
        await SaveWarningsAsync();
    }

    public static void SaveData()
    {
        Task.WaitAll(SaveUsersAsync(), SaveWarningsAsync());
    }

    private static T Deserialize<T>(string path)
    {
        var text = System.IO.File.ReadAllText(Path.Combine(ExecutablePath, path));
        return JsonConvert.DeserializeObject<T>(text) ?? throw new Exception($"U fucker changed {path} file...");
    }

    private static Task Serialize(object value, string path)
    {
        var text = JsonConvert.SerializeObject(value, Formatting.Indented);
        return System.IO.File.WriteAllTextAsync(Path.Combine(ExecutablePath, path), text, Encoding.UTF8);
    }
}
