namespace TelegramWarnBot;

public static class IOHandler
{
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
        if (Configuration is not null)
            return Configuration;

        var bytes = System.IO.File.ReadAllBytes("Data\\Configuration.json");
        return JsonSerializer.Deserialize<Configuration>(bytes);
    }

    public static Warnings GetWarnings()
    {
        if (Warnings is not null)
            return Warnings;

        var bytes = System.IO.File.ReadAllBytes("Data\\Warnings.json");
        return JsonSerializer.Deserialize<Warnings>(bytes) ?? throw new Exception("U fucker changed Warnings file...");
    }

    public static Task SaveWarningsAsync()
    {
        return Task.Run(() =>
        {
            var jsonBytes = JsonSerializer.SerializeToUtf8Bytes(Warnings, new JsonSerializerOptions() { WriteIndented = true, });
            System.IO.File.WriteAllBytes("Data\\Warnings.json", jsonBytes);
        });
    }

    public static List<UserDTO> GetUsers()
    {
        if (Users is not null)
            return Users;

        var bytes = System.IO.File.ReadAllBytes("Data\\Users.json");
        return JsonSerializer.Deserialize<List<UserDTO>>(bytes) ?? throw new Exception("U fucker changed Users file...");
    }

    public static Task SaveUsersAsync()
    {
        var jsonBytes = JsonSerializer.SerializeToUtf8Bytes(Users, new JsonSerializerOptions() { WriteIndented = true, });
        return System.IO.File.WriteAllBytesAsync("Data\\Users.json", jsonBytes);
    }

    public static Task RegisterClientAsync(long id, string username, string name)
    {
        // Adding client to list if not exist
        return Task.Run(() =>
        {
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
        });
    }
}

public class Configuration
{
    public string Token { get; set; }
}

public class Warnings
{
    public List<ChatDTO> ChatDTOs { get; set; }
}

public class ChatDTO
{
    public long Id { get; set; }
    public string Name { get; set; }
    public List<WarnedUserDTO> WarnedUsers { get; set; }
}

public class WarnedUserDTO
{
    public string Username { get; set; }
    public string Name { get; set; }
    public long Id { get; set; }
    public int WarnedCount { get; set; }
}

public class UserDTO
{
    public string Username { get; set; }
    public string Name { get; set; }
    public long Id { get; set; }
}
