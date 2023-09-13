namespace TelegramWarnBot;

public interface ISmartFormatterProvider
{
    SmartFormatter Formatter { get; }
}

public class SmartFormatterProvider : ISmartFormatterProvider
{
    public SmartFormatterProvider(SmartFormatter formatter)
    {
        Formatter = formatter;
    }

    public SmartFormatter Formatter { get; }
}