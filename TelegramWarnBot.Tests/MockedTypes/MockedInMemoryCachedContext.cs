namespace TelegramWarnBot;

public class MockedInMemoryCachedContext : IInMemoryCachedDataContext
{
    public static MockedInMemoryCachedContext Shared { get; }

    public List<MemberDTO> members = new();

    public List<MemberDTO> Members => members;

    static MockedInMemoryCachedContext()
    {
        Shared = new();
    }
}
