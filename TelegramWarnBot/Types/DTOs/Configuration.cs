﻿namespace TelegramWarnBot;

public class Configuration
{
    public int UpdateDelay { get; set; }
    public int MaxWarnings { get; set; }
    public bool DeleteWarnMessage { get; set; }
    public string OnBotJoinedChatMessage { get; set; }
    public Responses Captions { get; set; }
    public Trigger[] Triggers { get; set; }
}