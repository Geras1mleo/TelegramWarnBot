namespace TelegramWarnBot.Tests;

public class MessageHelperProvider
{
    public static IMessageHelper MessageHelper { get;  }

    static MessageHelperProvider()
    {
        MessageHelper = new MessageHelper();
    }
}
