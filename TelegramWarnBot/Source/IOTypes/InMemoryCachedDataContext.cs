namespace TelegramWarnBot;

public interface IInMemoryCachedDataContext
{
    List<MemberDTO> Members { get; }
}

public class InMemoryCachedDataContext : IOContextBase, IInMemoryCachedDataContext
{
    public InMemoryCachedDataContext(IHostEnvironment hostEnvironment) : base(hostEnvironment) { }
    public List<MemberDTO> Members { get; } = new();
}