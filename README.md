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
I will look at the **messages** in the chat and **respond/trigger** to the most offensive/provocative ones.

## Usage

### [Bot](TelegramWarnBot/Bot.json)

Replace *\<Telegram Bot Token\>* by your own token.

### [Configuration](TelegramWarnBot/Data/Configuration.json)

Here are some significant settings for the bot.
You can change them at runtime and then use `reload` in console to reload new configurations.

- [UpdateDelay](TelegramWarnBot/Data/Configuration.json#L2): Every given amount of seconds app will save all data of [Users](TelegramWarnBot/Data/Users.json) and [Chats](TelegramWarnBot/Data/Chats.json).
- [MaxWarnings](TelegramWarnBot/Data/Configuration.json#L3): Maximum number of warnings a member can receive before being banned.
- [DeleteWarnMessage](TelegramWarnBot/Data/Configuration.json#L4): Whether command message (`/warn @Geras1mleo` from administrator) needs to be deleted.
- [Captions](TelegramWarnBot/Data/Configuration.json#L5): The following parameters indicate the reactions of bot on certain events:
  - [OnBotJoinedChatMessage](TelegramWarnBot/Data/Configuration.json#L6): Greeting message that will be sent when bot is added to chat.
  - [NoPermissions](TelegramWarnBot/Data/Configuration.json#L7): Non-admin user attempts to warn group member.
  - [BotHasNoPermissions](TelegramWarnBot/Data/Configuration.json#L8): Bot require admin rights to warn and ban members (further).
  - [UserNotSpecified](TelegramWarnBot/Data/Configuration.json#L9): Use of command without mentioning the user.
  - [UserNotFound](TelegramWarnBot/Data/Configuration.json#L10): Mentioned user has been not found in this chat.
  - [Angry](TelegramWarnBot/Data/Configuration.json#L11): Attempt to use a command on the *bot itself*.
  - [BotWarnAttempt](TelegramWarnBot/Data/Configuration.json#L12): Attempt to use a command on *another bot*.
  - [UserHasNoWarnings](TelegramWarnBot/Data/Configuration.json#L13): Attempt to use */unwarn* on a user without warning.
  - [WarnedSuccessfully](TelegramWarnBot/Data/Configuration.json#L14): Post */warn* message that will mention warned user and his current amount of warnings.
  - [UnwarnedSuccessfully](TelegramWarnBot/Data/Configuration.json#L15): Post */unwarn* message that will mention unwarned user and his current amount of warnings.
  - [BannedSuccessfully](TelegramWarnBot/Data/Configuration.json#L16): Post */warn* message that will mention banned user.
- [Triggers](TelegramWarnBot/Data/Configuration.json#L18): Messages that will trigger the bot and send a response to corresponding chat with a triggered message attached in **reply** of response message:
  - [Messages](TelegramWarnBot/Data/Configuration.json#L20): Messages array that will trigger the bot.
  - [Response](TelegramWarnBot/Data/Configuration.json#L21): Reaction of the bot to the member who triggered it.
  - [MatchCase](TelegramWarnBot/Data/Configuration.json#L22): Whether message must match upper/lower case to trigger.
  - [MatchWholeMessage](TelegramWarnBot/Data/Configuration.json#L23): Whether message must match whole message to trigger.


### Console Features

**send** => Send message:<br/>
**-c** => Chat with according chat ID. Use **.** to send to all chats.<br/>
**-m** => Message to send. Please use **""** to indicate message. Markdown formating allowed.<br/>
Example: **send -c 123456 -m "Example message"**

## Like the project?

Give it a :star: Star!

## Found a bug?

Drop to <a href="https://github.com/Geras1mleo/TelegramWarnBot/issues">Issues</a><br/>
Or: sviatoslav.harasymchuk@gmail.com<br/>
<br/>
Thanks in advance!