namespace TelegramWarnBot.Types;

public enum ResponseType
{
    Succes,
    Error,
    Unhandled,
}

public class BotResponse
{
    public ResponseType Type { get; }
    public string Data { get; }

    public BotResponse(ResponseType type, string data)
    {
        Type = type;
        Data = data;
    }
}

public static class Responses // todo from config
{
    public const string NoPermissions = "🤭 У вас недостатньо прав для виконання цієї команди";

    public const string BotHasNoPermissions = "😢 У мене недостатньо прав...";

    public const string UserNotSpecified = "💀 Щоб використати цю функцію потрібно вказати жертву *@...* або відповісти на повідомлення командою */warn*";

    public const string UserNotFound = "🧐 Користувача не знайдено";

    public const string Angry = "🤬🤬🤬";

    public const string BotWarnAttempt = "🤖 Бот-брат";

    public const string UserHasNoWarnings = "😇 У цього користувача відсутні попередження!";

    public const string WarnedSuccessfully = "*🎁 Час подарунків!*\n\n{warnedUser}, Ви отримали *{warnedUser.WarnedCount} попередження з 3-х* від адміністрації групи.\n\nСкоріш за все, Ви порушили правила чату або відзначились несприйнятною поведінкою. Якщо зберете всі попередження — Ваше перебування у цій групі буде примусово припинено.";

    public const string UnwarnedSuccessfully = "{warnedUser}, Ваше попередження було скасовано, подаруночок конфісковано. Сорьки 🥺\n\nАктуальних попереджень {warnedUser.WarnedCount}.";

    public const string BannedSuccessfully = "*🎁 Час подарунків!*\n\nУчасник {warnedUser} зібрав переможну колекцію попереджень та отримує путівку з цієї групи в один кінець.";
}
