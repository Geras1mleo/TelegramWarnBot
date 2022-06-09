<p align="center">
  <img width="160" src="https://user-images.githubusercontent.com/67554762/171271199-bde4b277-b109-4aa4-ae6c-00546d844847.png">
</p>
<h1 align="center">Telegram Warn Bot</h1>
<p align="center">
  Telegram Warn Bot made with C# and &hearts; by Geras1mleo
</p>

## What can I do?
### I am a bot-moderator...
I keep track of the **warnings** and automatically **ban members** when the maximum amount of warnings has been reached.

**Promote me to _admin_** and **/warn** the bad guy by replying to his message or by mentioning **@bad_guy** in your command.
I will ban users who receive more than a certain number of warnings specified in [Configuration.json](TelegramWarnBot/Data/Configuration.json#L3).

The default value of [MaxWarnings](/TelegramWarnBot/Data/Configuration.json#L3) is *2*, which means that the user will be banned on his *3<sup>rd</sup>* warning.

If the bad guy behaves less badly, you can **/unwarn** him in the same way. If a member has already been banned, I will **unban** him so he can get back into the group.

**The commands /warn and /unwarn are only available to _administrators_ and the _owner_ of the group.**

#### Triggers
I will look at the **messages** in one specific chat (or any chat) and **respond/trigger** to the most offensive/provocative/funny ones.

#### Illegal Triggers
I will look at the **messages** in the chat and **notify the admin** (in private messages) if something illegal has been sent in one specific chat (or any chat).

## Usage

**Modify json configuration files according to your needs:**

### [Bot](TelegramWarnBot/Bot.json)

Replace *\<Telegram Bot Token\>* by your own token.

### [Configuration](TelegramWarnBot/Configuration/Configuration.json)

Here are some significant settings for the bot.
You can change them at runtime and then use `reload` in console to reload new configurations.

- [UpdateDelay](TelegramWarnBot/Configuration/Configuration.json#L2): Every given amount of seconds app will save all data of [Users](TelegramWarnBot/Data/Users.json) and [Chats](TelegramWarnBot/Data/Chats.json).
- [MaxWarnings](TelegramWarnBot/Configuration/Configuration.json#L3): Maximum number of warnings a member can receive before being banned.
- [DeleteWarnMessage](TelegramWarnBot/Configuration/Configuration.json#L4): Whether command message (`/warn @Geras1mleo` from administrator) needs to be deleted.
- [Captions](TelegramWarnBot/Configuration/Configuration.json#L5): The following parameters indicate the reactions of bot on certain events:
  - [OnBotJoinedChatMessage](TelegramWarnBot/Configuration/Configuration.json#L6): Greeting message that will be sent when bot is added to chat.
  - [NoPermissions](TelegramWarnBot/Configuration/Configuration.json#L7): Non-admin user attempts to warn group member.
  - [BotHasNoPermissions](TelegramWarnBot/Configuration/Configuration.json#L8): Bot require admin rights to warn and ban members (further).
  - [UserNotSpecified](TelegramWarnBot/Configuration/Configuration.json#L9): Use of command without mentioning the user.
  - [UserNotFound](TelegramWarnBot/Configuration/Configuration.json#L10): Mentioned user has been not found in this chat.
  - [Angry](TelegramWarnBot/Configuration/Configuration.json#L11): Attempt to use a command on the *bot itself*.
  - [BotWarnAttempt](TelegramWarnBot/Configuration/Configuration.json#L12): Attempt to use a command on *another bot*.
  - [UserHasNoWarnings](TelegramWarnBot/Configuration/Configuration.json#L13): Attempt to use */unwarn* on a user without warning.
  - [WarnedSuccessfully](TelegramWarnBot/Configuration/Configuration.json#L14): Post */warn* message that will mention warned user and his current amount of warnings.
  - [UnwarnedSuccessfully](TelegramWarnBot/Configuration/Configuration.json#L15): Post */unwarn* message that will mention unwarned user and his current amount of warnings.
  - [BannedSuccessfully](TelegramWarnBot/Configuration/Configuration.json#L16): Post */warn* message that will mention banned user.
  - [IllegalTriggerWarned](TelegramWarnBot/Configuration/Configuration.json#L17): *Illegal trigger warning* that will mention warned user and his current amount of warnings.
  - [IllegalTriggerBanned](TelegramWarnBot/Configuration/Configuration.json#L18): *Illegal trigger warning* that will mention banned user.

### [Triggers](TelegramWarnBot/Configuration/Triggers.json)

Messages that will trigger the bot and send a response to corresponding chat with a triggered message attached in **reply** of response message.

- [Chat](TelegramWarnBot/Configuration/Triggers.json#L3): Chat to which the trigger is applicable or *null* (any chat).
- [Messages](TelegramWarnBot/Configuration/Triggers.json#L4): Messages array that will trigger the bot.
- [Response](TelegramWarnBot/Configuration/Triggers.json#L5): Reaction of the bot to the member who triggered it.
- [MatchCase](TelegramWarnBot/Configuration/Triggers.json#L6): Whether message must match upper/lower case to trigger.
- [MatchWholeMessage](TelegramWarnBot/Configuration/Triggers.json#L7): Whether message must match whole message to trigger.

### [IllegalTriggers](TelegramWarnBot/Configuration/IllegalTriggers.json#L48)

Notification is sent to admins (optional *Warn* sender) when an **illegal word** is sent in a specific chat (or any chat).

- [Chat](TelegramWarnBot/Configuration/IllegalTriggers.json#L3): Chat to which the notification is applicable or *null* (any chat).
- [WarnMember](TelegramWarnBot/Configuration/IllegalTriggers.json#L4): Whether the sender of the message is going to receive a *warning* ([IllegalTriggerWarned](TelegramWarnBot/Configuration/Configuration.json#L17) or [IllegalTriggerBanned](TelegramWarnBot/Configuration/Configuration.json#L18) will be sent).
- [DeleteMessage](TelegramWarnBot/Configuration/IllegalTriggers.json#L5): Whether the message with illegal word needs to be deleted.
- [IllegalWords](TelegramWarnBot/Configuration/IllegalTriggers.json#L6): Words array that will trigger the notification.
- [NotifiedAdmins](TelegramWarnBot/Configuration/IllegalTriggers.json#L7): Array of IDs of *administrators* that will receive the notification.

### Console Features

- **send** => Send message:
  - **-c** => Chat with according chat ID. Use **.** to send to all chats.
  - **-m** => Message to send. Please use **""** to indicate message. Markdown formating allowed.

Example: **send -c 123456 -m "Example message"**

## Like the project?

Give it a :star: Star!

## Found a bug?

Drop to <a href="https://github.com/Geras1mleo/TelegramWarnBot/issues">Issues</a><br/>
Or: sviatoslav.harasymchuk@gmail.com<br/>
<br/>
Thanks in advance!
