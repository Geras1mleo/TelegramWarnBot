namespace TelegramWarnBot;

public static class Extensions
{
    public static bool Validate(this Update update)
    {
        Chat chat = update.GetChat();

        if (update.GetFromUser() is null)
            return false;

        if (chat is not null)
        {
            return chat.Type == ChatType.Group || chat.Type == ChatType.Supergroup;
        }

        return false;
    }

    public static Chat GetChat(this Update update)
    {
        return update.Type switch
        {
            UpdateType.Message => update.Message.Chat,
            UpdateType.ChatMember => update.ChatMember.Chat,
            UpdateType.MyChatMember => update.MyChatMember.Chat,
            _ => null
        };
    }

    public static User GetFromUser(this Update update)
    {
        return update.Type switch
        {
            UpdateType.Message => update.Message.From,
            UpdateType.ChatMember => update.ChatMember.From,
            UpdateType.MyChatMember => update.MyChatMember.From,
            _ => null
        };
    }

    public static ChatMember GetOldMember(this Update update)
    {
        return update.Type switch
        {
            UpdateType.ChatMember => update.ChatMember.OldChatMember,
            UpdateType.MyChatMember => update.MyChatMember.OldChatMember,
            _ => null
        };
    }

    public static ChatMember GetNewMember(this Update update)
    {
        return update.Type switch
        {
            UpdateType.ChatMember => update.ChatMember.NewChatMember,
            UpdateType.MyChatMember => update.MyChatMember.NewChatMember,
            _ => null
        };
    }

    public static bool IsValidCommand(this string message)
    {
        var parts = message.Split(' ');
        return parts.Length > 0 && parts[0].StartsWith('/');
    }

    public static string GetFullName(this User user)
    {
        return (user.FirstName + " " + user.LastName).Trim();
    }
}
