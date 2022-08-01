namespace TelegramWarnBot;

public interface ICachedDataContext
{
    List<ChatDTO> Chats { get; }
    List<UserDTO> Users { get; }
    List<ChatWarnings> Warnings { get; }
    List<MemberDTO> Members { get; }

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
    private List<ChatWarnings> warnings;
    private List<UserDTO> users;
    private List<ChatDTO> chats;
    private readonly List<MemberDTO> members = new();

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

    public List<MemberDTO> Members => members;

    private Task SaveWarningsAsync()
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

    private Task SaveUsersAsync()
    {
        return SerializeAsync(Users, Path.Combine("Data", "Users.json"));
    }

    private Task SaveChatsAsync()
    {
        return SerializeAsync(Chats, Path.Combine("Data", "Chats.json"));
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
            chatDto = new()
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
            while (true)
            {
                await Task.Delay(delaySeconds * 1000, cancellationToken);
                await SaveUsersAsync();
                await SaveWarningsAsync();
                await SaveChatsAsync();
            }
        }, cancellationToken);
    }

    public void SaveData()
    {
        Task.WaitAll(SaveUsersAsync(), SaveWarningsAsync(), SaveChatsAsync());
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
}
