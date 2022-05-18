namespace TelegramWarnBot;

public static class IOHandler
{
    // todo reset
    private static Warnings Warnings;
    private static List<UserDTO> Users;
    private static Configuration Configuration;

    static IOHandler()
    {
        Warnings = GetWarnings();
        Users = GetUsers();
        Configuration = GetConfiguration();
    }

    public static Configuration GetConfiguration()
    {
        if (Configuration is not null) return Configuration;

        return Deserialize<Configuration>("Data\\Configuration.json");
    }

    public static Warnings GetWarnings()
    {
        if (Warnings is not null) return Warnings;

        return Deserialize<Warnings>("Data\\Warnings.json");
    }

    private static Task SaveWarningsAsync()
    {
        return Serialize(Warnings, "Data\\Warnings.json");
    }

    public static List<UserDTO> GetUsers()
    {
        if (Users is not null) return Users;

        return Deserialize<List<UserDTO>>("Data\\Users.json");
    }

    private static Task SaveUsersAsync()
    {
        return Serialize(Users, "Data\\Users.json");
    }

    public static void RegisterClient(long id, string username, string name)
    {
        // Adding client to list if not exist

        var user = Users.FirstOrDefault(u => u.Id == id);
        if (user is null)
        {
            user = new()
            {
                Id = id,
                Username = username?.ToLower(),
                Name = name,
            };
            Users.Add(user);
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

    public static async Task SaveDataAsync()
    {
        await IOHandler.SaveUsersAsync();
        await IOHandler.SaveWarningsAsync();
    }

    public static void SaveData()
    {
        Task.WaitAll(IOHandler.SaveUsersAsync(), IOHandler.SaveWarningsAsync());
    }

    public static T Deserialize<T>(string path)
    {
        var text = System.IO.File.ReadAllText(path);
        return JsonConvert.DeserializeObject<T>(text) ?? throw new Exception($"U fucker changed {path} file...");
    }

    private static Task Serialize(object value, string path)
    {
        var text = JsonConvert.SerializeObject(value, Formatting.Indented);
        return System.IO.File.WriteAllTextAsync(path, text, Encoding.UTF8);
    }
}
