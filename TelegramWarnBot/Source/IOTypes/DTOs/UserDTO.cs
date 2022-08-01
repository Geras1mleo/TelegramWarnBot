namespace TelegramWarnBot;

public class UserDTO
{
    public string Username { get; set; }

    private string firstName;
    public string FirstName
    {
        get => firstName;
        set
        {
            // There is a bug somewhere that clears firstname of the user.. (only in production)
            // It is a only way to find it...

            if (string.IsNullOrEmpty(value))
                throw new ArgumentException("Clearing name of user");

            firstName = value;
        }
    }
    public string LastName { get; set; }
    public long Id { get; set; }

    private string _fullName = null;

    public string GetName()
    {
        if (_fullName is not null)
            return _fullName;

        return _fullName = $"{FirstName}{LastName?.Insert(0, " ")}";
    }

    public override string ToString()
    {
        return $"{(Username is null ? $"[{GetName()}](tg://user?id={Id})" : $"@{Username.Replace("_", @"\_")}")}";
    }
}

public class MentionedUserDTO : UserDTO
{
    public int? Warnings { get; set; }
}
