namespace TelegramWarnBot;

public interface ISmartFormatterProvider
{
    SmartFormatter Formatter { get; }
}

public class SmartFormatterProvider : ISmartFormatterProvider
{
    public SmartFormatter Formatter { get; private set; }

    public SmartFormatterProvider(SmartFormatter formatter)
    {
        Formatter = formatter;
    }
}
