﻿namespace TelegramWarnBot;

public class ChatDTO
{
    public long Id { get; set; }
    public string Name { get; set; }
    public List<WarnedUser> WarnedUsers { get; set; }
}
