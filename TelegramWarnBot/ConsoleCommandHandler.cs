namespace TelegramWarnBot;

public static class CommandHandler
{
    public static bool Send(TelegramBotClient client, List<string> parameters, CancellationToken cancellationToken)
    {
        int chatIndex = parameters.FindIndex(p => p.ToLower() == "-c");

        if (chatIndex < 0)
            return false;

        string chatParameter = parameters.ElementAtOrDefault(chatIndex + 1);

        bool broadcast = chatParameter == ".";

        if (!broadcast && !long.TryParse(chatParameter, out long chatId))
            return false;

        int messageIndex = parameters.FindIndex(p => p.ToLower() == "-m");

        if (messageIndex < 0)
            return false;

        string message = parameters.ElementAtOrDefault(messageIndex + 1);

        if (message is null || !message.StartsWith("\"") || !message.EndsWith("\"") || message.Length == 1)
            return false;

        message = message.Substring(1, message.Length - 2);

        var chats = new List<ChatDTO>();

        if(broadcast)
            chats = IOHandler.GetWarnings().Chats;
        else
        {
            var chat = IOHandler.GetWarnings().Chats.FirstOrDefault(c => c.Id == chatId);
            
            if(chat is not null) 
                chats.Add(chat);
        }
            
        // todo test
        
        for(int i = 0; i < chats.count; i++)
        {
            client.SendTextMessageAsync(chats[i].Id, message, cancellationToken: cancellationToken, parseMode: ParseMode.Markdown);   
        }

        return false;
    }
}