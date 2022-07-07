namespace TelegramWarnBot;

public static class IOHandler
{
    private static readonly string ExecutablePath = AppDomain.CurrentDomain.BaseDirectory;

    // Caching all configs
    private static Configuration configuration;
    private static Trigger[] triggers;
    private static IllegalTrigger[] illegalTriggers;

    private static List<ChatWarnings> warnings;
    private static List<UserDTO> users;
    private static List<ChatDTO> chats;

    private static List<BotError> logs;

    public static BotConfiguration BotConfiguration
    {
        get
        {
            var config = Deserialize<BotConfiguration>("Bot.json");
            config.RegisteredChats = Deserialize<List<long>>(Path.Combine("Configuration", "RegisteredChats.json"));
            return config;
        }
    }

    public static Configuration Configuration
    {
        get
        {
            if (configuration is null)
                configuration = Deserialize<Configuration>(Path.Combine("Configuration", "Configuration.json"));

            return configuration;
        }
    }

    public static Trigger[] Triggers
    {
        get
        {
            if (triggers is null)
                triggers = Deserialize<Trigger[]>(Path.Combine("Configuration", "Triggers.json"));

            return triggers;
        }
    }

    public static IllegalTrigger[] IllegalTriggers
    {
        get
        {
            if (illegalTriggers is null)
                illegalTriggers = Deserialize<IllegalTrigger[]>(Path.Combine("Configuration", "IllegalTriggers.json"));

            return illegalTriggers;
        }
    }

    public static List<UserDTO> Users
    {
        get
        {
            if (users is null)
                users = Deserialize<List<UserDTO>>(Path.Combine("Data", "Users.json"));

            return users;
        }
    }

    public static List<ChatDTO> Chats
    {
        get
        {
            if (chats is null)
                chats = Deserialize<List<ChatDTO>>(Path.Combine("Data", "Chats.json"));

            return chats;
        }
    }

    public static List<ChatWarnings> Warnings
    {
        get
        {
            if (warnings is null)
                warnings = Deserialize<List<ChatWarnings>>(Path.Combine("Data", "ChatWarnings.json"));

            return warnings;
        }
    }

    public static List<BotError> Logs
    {
        get
        {
            if (logs is null)
                logs = Deserialize<List<BotError>>(Path.Combine("Data", "Logs.json"));

            return logs;
        }
    }

    public static void ReloadConfiguration()
    {
        configuration = null;
        triggers = null;
        illegalTriggers = null;
        Bot.Configuration = BotConfiguration;
    }

    private static Task SaveWarningsAsync()
    {
        // Clear up first
        foreach (var warning in Warnings)
        {
            for (int i = warning.WarnedUsers.Count - 1; i >= 0; i--)
            {
                if (warning.WarnedUsers[i].Warnings < 1)
                    warning.WarnedUsers.RemoveAt(i);
            }
        }
        for (int i = Warnings.Count - 1; i >= 0; i--)
        {
            if (Warnings[i].WarnedUsers.Count < 1)
                Warnings.RemoveAt(i);
        }

        return SerializeAsync(Warnings, Path.Combine("Data", "ChatWarnings.json"));
    }

    private static Task SaveUsersAsync()
    {
        return SerializeAsync(Users, Path.Combine("Data", "Users.json"));
    }

    private static Task SaveChatsAsync()
    {
        return SerializeAsync(Chats, Path.Combine("Data", "Chats.json"));
    }

    public static Task SaveRegisteredChatsAsync()
    {
        return SerializeAsync(Bot.Configuration.RegisteredChats, Path.Combine("Configuration", "RegisteredChats.json"));
    }

    public static Task SaveLogsAsync()
    {
        return SerializeAsync(Logs, Path.Combine("Data", "Logs.json"));
    }

    public static void CacheUser(User user)
    {
        // Adding client to list if not exist
        var userDto = Users.FirstOrDefault(u => u.Id == user.Id);
        if (userDto is null)
        {
            userDto = new()
            {
                Id = user.Id,
                Username = user.Username?.ToLower(),
                Name = user.GetFullName(),
            };
            Users.Add(userDto);
        }
        else if (userDto.Username != user.Username?.ToLower()
              || userDto.Name != user.GetFullName())
        {
            userDto.Username = user.Username?.ToLower();
            userDto.Name = user.GetFullName();
        }
    }

    public static void CacheChat(Chat chat, long[] admins)
    {
        // Adding chat to list if not exist
        var chatDto = Chats.FirstOrDefault(c => c.Id == chat.Id);
        if (chatDto is null)
        {
            chatDto = new()
            {
                Id = chat.Id,
                Name = chat.Title,
                Admins = admins
            };
            Chats.Add(chatDto);
        }
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
        Task.WaitAll(SaveUsersAsync(), SaveWarningsAsync(), SaveChatsAsync(), SaveLogsAsync());
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
