﻿namespace TelegramWarnBot;

public class ChatDTO
{
    public long Id { get; set; }
    public string Name { get; set; }
    public List<WarnedUserDTO> WarnedUsers { get; set; }
}
