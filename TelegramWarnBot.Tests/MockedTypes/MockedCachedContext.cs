namespace TelegramWarnBot.Tests;

public class MockedCachedContext : ICachedDataContext
{
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
        }
    };

    public List<ChatWarnings> Warnings => throw new NotImplementedException();

    public List<MemberDTO> Members => throw new NotImplementedException();

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
}
