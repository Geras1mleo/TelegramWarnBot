public static class CommandHandler
{
    public static bool Send(TelegramBotClient client, string[] params)
    {
        // todo
        return false;
    }

    public static void Exit(CancellationToken cancellationToken)
    {
        IOHandler.SaveDataAsync().GetAwaiter().GetResult();
        cancellationToken.Cancel();
        Environment.Exit(1);
    }
}