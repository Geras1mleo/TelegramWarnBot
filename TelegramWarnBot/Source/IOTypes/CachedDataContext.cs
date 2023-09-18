namespace TelegramWarnBot;

public interface ICachedDataContext
{
    List<ChatDTO> Chats { get; }
    List<UserDTO> Users { get; }
    List<ChatWarnings> Warnings { get; }
    List<DeletedMessageLog> Spam { get; }
    List<DeletedMessageLog> Illegal { get; }

    void BeginUpdate(int delaySeconds, CancellationToken cancellationToken);
    ChatDTO CacheChat(Chat chat, List<long> admins);
    UserDTO CacheUser(User user);

    void SaveData();
    Task SaveRegisteredChatsAsync(List<long> registeredChats);

    ChatDTO FindChatById(long id);
    UserDTO FindUserById(long id);
    ChatWarnings FindWarningByChatId(long chatId);
}

public class CachedDataContext : IOContextBase, ICachedDataContext
{
    private List<ChatDTO> chats;
    private List<DeletedMessageLog> illegal;
    private List<DeletedMessageLog> spam;
    private List<UserDTO> users;
    private List<ChatWarnings> warnings;

    public CachedDataContext(IHostEnvironment hostEnvironment) : base(hostEnvironment) { }

    public List<UserDTO> Users
    {
        get
        {
            if (users is null)
                users = Deserialize<List<UserDTO>>(Path.Combine("Data", "Users.json"));

            return users;
        }
    }

    public List<ChatDTO> Chats
    {
        get
        {
            if (chats is null)
                chats = Deserialize<List<ChatDTO>>(Path.Combine("Data", "Chats.json"));

            return chats;
        }
    }

    public List<ChatWarnings> Warnings
    {
        get
        {
            if (warnings is null)
                warnings = Deserialize<List<ChatWarnings>>(Path.Combine("Data", "ChatWarnings.json"));

            return warnings;
        }
    }

    public List<DeletedMessageLog> Spam
    {
        get
        {
            if (spam is null)
                spam = Deserialize<List<DeletedMessageLog>>(Path.Combine("Data", "Spam.json"));

            return spam;
        }
    }

    public List<DeletedMessageLog> Illegal
    {
        get
        {
            if (illegal is null)
                illegal = Deserialize<List<DeletedMessageLog>>(Path.Combine("Data", "Illegal.json"));

            return illegal;
        }
    }

    public Task SaveRegisteredChatsAsync(List<long> registeredChats)
    {
        return SerializeAsync(registeredChats, Path.Combine("Configuration", "RegisteredChats.json"));
    }

    public UserDTO CacheUser(User user)
    {
        // Adding client to list if not exist
        var userDto = FindUserById(user.Id);
        if (userDto is null)
        {
            userDto = user.Map();
            Users.Add(userDto);
        }
        else if (userDto.Username != user.Username
              || userDto.FirstName != user.FirstName
              || userDto.LastName != user.LastName)
        {
            userDto.Username = user.Username;
            userDto.FirstName = user.FirstName;
            userDto.LastName = user.LastName;
        }
        return userDto;
    }

    public ChatDTO CacheChat(Chat chat, List<long> admins)
    {
        // Adding chat to list if not exist
        var chatDto = FindChatById(chat.Id);
        if (chatDto is null)
        {
            chatDto = new ChatDTO
            {
                Id = chat.Id,
                Name = chat.Title,
                Admins = admins
            };
            Chats.Add(chatDto);
        }
        return chatDto;
    }

    public void BeginUpdate(int delaySeconds, CancellationToken cancellationToken)
    {
        Task.Run(async () =>
        {
            Log.Information("Caching data each {seconds} seconds", delaySeconds);
            while (true)
            {
                await Task.Delay(delaySeconds * 1000, cancellationToken);
                await SaveUsersAsync();
                await SaveWarningsAsync();
                await SaveChatsAsync();
                await SaveSpamAsync();
                await SaveIllegalAsync();
            }
        }, cancellationToken);
    }

    public void SaveData()
    {
        Task.WaitAll(SaveUsersAsync(), SaveWarningsAsync(), SaveChatsAsync(), SaveSpamAsync(), SaveIllegalAsync());
    }

    public ChatDTO FindChatById(long id)
    {
        return Chats.Find(c => c.Id == id);
    }

    public UserDTO FindUserById(long id)
    {
        return Users.Find(u => u.Id == id);
    }

    public ChatWarnings FindWarningByChatId(long chatId)
    {
        return Warnings.Find(w => w.ChatId == chatId);
    }

    private Task SaveWarningsAsync()
    {
        // Clear up first
        foreach (var warning in Warnings)
            for (var i = warning.WarnedUsers.Count - 1; i >= 0; i--)
                if (warning.WarnedUsers[i].Warnings < 1)
                    warning.WarnedUsers.RemoveAt(i);
        for (var i = Warnings.Count - 1; i >= 0; i--)
            if (Warnings[i].WarnedUsers.Count < 1)
                Warnings.RemoveAt(i);

        return SerializeAsync(Warnings, Path.Combine("Data", "ChatWarnings.json"));
    }

    private Task SaveUsersAsync()
    {
        return SerializeAsync(Users, Path.Combine("Data", "Users.json"));
    }

    private Task SaveChatsAsync()
    {
        return SerializeAsync(Chats, Path.Combine("Data", "Chats.json"));
    }

    public Task SaveSpamAsync()
    {
        return SerializeAsync(Spam, Path.Combine("Data", "Spam.json"));
    }

    public Task SaveIllegalAsync()
    {
        return SerializeAsync(Illegal, Path.Combine("Data", "Illegal.json"));
    }
}