namespace TelegramWarnBot;

public class MockedInMemoryCachedContext : IInMemoryCachedDataContext
{
    public List<MemberDTO> members = new();

    public List<MemberDTO> Members => members;
}
