namespace TelegramWarnBot.Tests;

public class MockedCachedContext : ICachedDataContext
{
    public static MockedCachedContext Shared { get; }

    static MockedCachedContext()
    {
        Shared = new();
    }

    public List<ChatDTO> Chats => chats;

    private List<ChatDTO> chats = new()
    {
        new ChatDTO()
        {
            Id = 69,
            Admins = new List<long>()
            {
                654,
                99,
                402659130,
            },
            Name = "Bot Test"
        },
    };

    public List<UserDTO> Users => users;

    private List<UserDTO> users = new()
    {
        new UserDTO()
        {
            Id = 654,
            FirstName = "Admin",
            LastName = null,
            Username = "admin_of_the_chat"
        },
        new UserDTO()
        {
            Id = 420,
            FirstName = "Robert",
            LastName = "Johnson",
            Username = "robert_johnson"
        },
        new UserDTO()
        {
            Id = 510,
            FirstName = "Hugh",
            LastName = "Jackman",
            Username = null,
        }
    };

    public List<ChatWarnings> Warnings => warnings;

    private List<ChatWarnings> warnings = new()
    {
        new ChatWarnings()
        {
            ChatId = 69,
            WarnedUsers =new List<WarnedUser>()
            {
                new WarnedUser()
                {
                    Id = 510,
                    Warnings = 1,
                },
                new WarnedUser()
                {
                    Id = 420,
                    Warnings = 0,
                }
            }
        }
    };

    public void BeginUpdate(int delaySeconds, CancellationToken cancellationToken)
    { }

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

    public void SaveData()
    { }

    public Task SaveRegisteredChatsAsync(List<long> registeredChats)
    {
        return Task.CompletedTask;
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
