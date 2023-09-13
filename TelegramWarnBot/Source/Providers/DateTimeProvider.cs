namespace TelegramWarnBot;

public interface IDateTimeProvider
{
    public DateTime DateTimeNow { get; }
}

public class DateTimeProvider : IDateTimeProvider
{
    public DateTime DateTimeNow => DateTime.Now;
}