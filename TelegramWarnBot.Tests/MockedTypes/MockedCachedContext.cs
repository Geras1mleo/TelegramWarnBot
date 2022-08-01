namespace TelegramWarnBot.Tests;

public class MockedCachedContext : ICachedDataContext
{
    public static MockedCachedContext Shared { get; }

    static MockedCachedContext()
    {
        Shared = new();
    }

    public List<ChatDTO> Chats => new()
    {
        new ChatDTO()
        {
            Id = 69,
            Admins = new List<long>()
            {
                5149719899,
                713786114,
                402659130,
            }
        },
    };

    public List<UserDTO> Users => new()
    {
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

    public List<ChatWarnings> Warnings
    {
        get
        {
            return new List<ChatWarnings>()
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
                            Warnings = 2,
                        }
                    }
                }
            };
        }
    }

    private List<MemberDTO> members = new();
    public List<MemberDTO> Members => members;

    public void BeginUpdate(int delaySeconds, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public ChatDTO CacheChat(Chat chat, List<long> admins)
    {
        throw new NotImplementedException();
    }

    public UserDTO CacheUser(User user)
    {
        throw new NotImplementedException();
    }

    public void SaveData()
    {
        throw new NotImplementedException();
    }

    public Task SaveRegisteredChatsAsync(List<long> registeredChats)
    {
        throw new NotImplementedException();
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
