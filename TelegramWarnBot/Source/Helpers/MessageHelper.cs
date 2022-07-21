namespace TelegramWarnBot;

public interface IMessageHelper
{
    bool MatchCardNumber(string message);
    bool MatchLinkMessage(Message message);
    bool MatchMessage(string[] matchFromMessages, bool matchWholeMessage, bool matchCase, string message);
}

public class MessageHelper : IMessageHelper
{
    public bool MatchMessage(string[] matchFromMessages, bool matchWholeMessage, bool matchCase, string message)
    {
        if (matchWholeMessage)
            return matchFromMessages.Any(m => matchCase ? m == message : m.ToLower() == message.ToLower());

        return matchFromMessages.Any(m => message.Contains(m, matchCase ? StringComparison.CurrentCulture : StringComparison.CurrentCultureIgnoreCase));
    }

    public bool MatchLinkMessage(Message message)
    {
        return message.Entities?.Any(e => e.Type == MessageEntityType.Url || e.Type == MessageEntityType.TextLink
                                       || e.Type == MessageEntityType.TextMention || e.Type == MessageEntityType.Mention) ?? false;
    }

    public bool MatchCardNumber(string message)
    {
        return Tools.CardNumberRegex.Match(message).Success;
    }
}
