namespace TelegramWarnBot;

public static class Bot
{
    public static BotConfiguration Configuration;
    public static User User;
    public static TelegramBotClient Client;
}

public class BotConfiguration
{
    public string Token { get; set; }
}