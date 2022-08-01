namespace TelegramWarnBot;

public interface IInMemoryCachedDataContext
{
    List<MemberDTO> Members { get; }
}

public class InMemoryCachedDataContext : IOContextBase, IInMemoryCachedDataContext
{
    private readonly List<MemberDTO> members = new();
    public List<MemberDTO> Members => members;

    public InMemoryCachedDataContext(IHostEnvironment hostEnvironment) : base(hostEnvironment) { }
}
