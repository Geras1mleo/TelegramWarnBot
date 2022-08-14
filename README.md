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

First of all you have to add me into your chat. To use default bot, search for **@fancy_moderator_bot**

**Promote me to _admin_** and **/warn** the bad guy by replying to his message or by mentioning **@bad_guy** in your command.
I will ban users who receive more than a certain number of warnings specified in [Configuration.json](TelegramWarnBot/Configuration/Configuration.json#L3).

The default value of [MaxWarnings](TelegramWarnBot/Configuration/Configuration.json#L3) is *3*, which means that the user will be banned on his *3<sup>rd</sup>* warning.

If the bad guy behaves less badly, you can **/unwarn** him in the same way. If a member is already banned from chat, I will **unban** him so he can get back into the group.

All members can check their number of warnings by typing **/wcount**, or **/wcount @bad_guy** to check the number of warnings of someone else.

**The commands _/warn_ and _/unwarn_ are only available to _administrators_ and the _owner_ of the group.**

**I can also:**<br>
[delete](TelegramWarnBot/Configuration/Configuration.json#L5) **"User joined/left chat"** - messages...<br>
[delete](TelegramWarnBot/Configuration/Configuration.json#L6) spam (external links / URL's and mentions) of newly joined members (<24 hours in chat, See: [NewMemberStatusFromHours](TelegramWarnBot/Configuration/Configuration.json#L7))

#### Triggers
I will look at the **messages** in one specific chat (or any chat) and **respond/trigger** to the most offensive/provocative/funny ones.

#### Illegal Triggers
I will look at the **messages** in the chat and **notify admins** (in private messages) if something illegal has been sent in one specific chat (or any chat). I will **delete** messages with illegal words and optionally also give an **auto warning** to the sender. 

## Usage

**Modify json configuration files according to your needs:**

### [Bot](TelegramWarnBot/Bot.json)

Replace *\<Telegram Bot Token\>* by your own token.

### [Configuration](TelegramWarnBot/Configuration/Configuration.json)

Here are some significant settings for the bot.
You can change them at runtime and then use `reload` in console to reload new configurations.

- [UpdateDelay](TelegramWarnBot/Configuration/Configuration.json#L2): Every given amount of seconds app will save all data of [Users](TelegramWarnBot/Data/Users.json), [Chats](TelegramWarnBot/Data/Chats.json) and [ChatWarnings](TelegramWarnBot/Data/ChatWarnings.json).
- [MaxWarnings](TelegramWarnBot/Configuration/Configuration.json#L3): The number of warnings warned member will be banned on.
- [DeleteWarnMessage](TelegramWarnBot/Configuration/Configuration.json#L4): Whether command message (`/warn @Geras1mleo` from administrator) needs to be deleted.
- [DeleteJoinedLeftMessage](TelegramWarnBot/Configuration/Configuration.json#L5): Whether *"geras1mleo joined chat"* - message needs to be deleted.
- [DeleteLinksFromNewMembers](TelegramWarnBot/Configuration/Configuration.json#L6): Whether bot will delete spam messages (external links or mentions) of newly joined members (<24 hours in chat, see property below).
- [NewMemberStatusFromHours](TelegramWarnBot/Configuration/Configuration.json#L7): A member of the chat is considered a new member if he/she has been a member of the chat for less than the specified number of hours.
- [AllowAdminWarnings](TelegramWarnBot/Configuration/Configuration.json#L8): Whether *administrators* can receive *warnings*. 
- [Captions](TelegramWarnBot/Configuration/Configuration.json#L9): The following parameters indicate the reactions of bot on certain events:
  - [OnBotJoinedChatMessage](TelegramWarnBot/Configuration/Configuration.json#L10): Greeting message that will be sent when bot is added to chat.
  - [ChatNotRegistered](TelegramWarnBot/Configuration/Configuration.json#L11): When the bot is being used in a chat, it is not intended to be a member of.
  - [UserNoPermissions](TelegramWarnBot/Configuration/Configuration.json#L12): Non-admin user attempts to warn chat member.
  - [BotHasNoPermissions](TelegramWarnBot/Configuration/Configuration.json#L13): Bot require admin rights to warn and ban members.
  - [InvalidOperation](TelegramWarnBot/Configuration/Configuration.json#L14): Use of command (*/warn* or */unwarn*) without mentioning the user or replying to some (suspicious) message.
  - [UserNotFound](TelegramWarnBot/Configuration/Configuration.json#L15): Mentioned user has been not found in this chat.
  - [WarnedSuccessfully](TelegramWarnBot/Configuration/Configuration.json#L16): Post */warn* message that will mention warned user and his current amount of warnings.
  - [UnwarnedSuccessfully](TelegramWarnBot/Configuration/Configuration.json#L17): Post */unwarn* message that will mention unwarned user and his current amount of warnings.
  - [BannedSuccessfully](TelegramWarnBot/Configuration/Configuration.json#L18): Post */warn* message that will mention banned user.
  - [UnwarnUserNoWarnings](TelegramWarnBot/Configuration/Configuration.json#L19): Attempt to use */unwarn* on a user without any warnings.
  - [WarnAdminAttempt](TelegramWarnBot/Configuration/Configuration.json#L20): Attempt to use */warn* on administrator when *AllowAdminWarnings = false*.
  - [UnwarnAdminAttempt](TelegramWarnBot/Configuration/Configuration.json#L21): Attempt to use */unwarn* on administrator when *AllowAdminWarnings = false*.
  - [WarnBotAttempt](TelegramWarnBot/Configuration/Configuration.json#L22): Attempt to use */warn* on *another bot*.
  - [UnwarnBotAttempt](TelegramWarnBot/Configuration/Configuration.json#L23): Attempt to use */unwarn* on *another bot*.
  - [WarnBotSelfAttempt](TelegramWarnBot/Configuration/Configuration.json#L24): Attempt to use */warn* on the *bot itself*.
  - [UnwarnBotSelfAttempt](TelegramWarnBot/Configuration/Configuration.json#L25): Attempt to use */unwarn* on the *bot itself*.
  - [IllegalTriggerWarned](TelegramWarnBot/Configuration/Configuration.json#L26): Automatic *Illegal trigger warning* that will mention warned user and his current amount of warnings.
  - [IllegalTriggerBanned](TelegramWarnBot/Configuration/Configuration.json#L27): Automatic *Illegal trigger warning* that will mention banned user.
  - [WCountMessage](TelegramWarnBot/Configuration/Configuration.json#L28): Post */wcount* message that will mention user and his amount of warnings (only when > 0).
  - [WCountUserHasNoWarnings](TelegramWarnBot/Configuration/Configuration.json#L29): Post */wcount* message that will mention user, user has any warnings (= 0).
  - [WCountAdminAttempt](TelegramWarnBot/Configuration/Configuration.json#L30): Attempt to use */wcount* on administrator when *AllowAdminWarnings = false*.
  - [WCountBotAttempt](TelegramWarnBot/Configuration/Configuration.json#L31): Attempt to use */wcount* on *another bot*.
  - [WCountBotSelfAttempt](TelegramWarnBot/Configuration/Configuration.json#L32): Attempt to use */wcount* on the *bot itself*.

### [Triggers](TelegramWarnBot/Configuration/Triggers.json)

Messages that will trigger the bot and send a response to corresponding chat with a triggered message attached in **reply** of response message.

- [Chat](TelegramWarnBot/Configuration/Triggers.json#L3): Chat to which the trigger is applicable or *null* (any chat).
- [Messages](TelegramWarnBot/Configuration/Triggers.json#L4): Messages array that will trigger the bot.
- [Responses](TelegramWarnBot/Configuration/Triggers.json#L5): Reactions (1 random response from set) of the bot to the member who triggered it.
- [MatchCase](TelegramWarnBot/Configuration/Triggers.json#L6): Whether message must match upper/lower case to trigger.
- [MatchWholeMessage](TelegramWarnBot/Configuration/Triggers.json#L7): Whether message must match whole message to trigger.

### [IllegalTriggers](TelegramWarnBot/Configuration/IllegalTriggers.json#L48)

Notification is sent to admins (optional *Warn* sender) when an **illegal word** is sent in a specific chat (or any chat).

- [Chat](TelegramWarnBot/Configuration/IllegalTriggers.json#L3): Chat to which the notification is applicable or *null* (any chat).
- [WarnMember](TelegramWarnBot/Configuration/IllegalTriggers.json#L4): Whether the sender of the message is going to receive a *warning* ([IllegalTriggerWarned](TelegramWarnBot/Configuration/Configuration.json#L22) or [IllegalTriggerBanned](TelegramWarnBot/Configuration/Configuration.json#L23) according to warnings amount).
- [DeleteMessage](TelegramWarnBot/Configuration/IllegalTriggers.json#L5): Whether the message with illegal word needs to be deleted.
- [IgnoreAdmins](TelegramWarnBot/Configuration/IllegalTriggers.json#L6): Whether the trigger will ignore messages from administrators.
- [IllegalWords](TelegramWarnBot/Configuration/IllegalTriggers.json#L7): Words array that will trigger the notification.
- [NotifiedAdmins](TelegramWarnBot/Configuration/IllegalTriggers.json#L8): Array of IDs of *administrators* that will receive the notification.

### Console Features

- **send \<Message\>** => Sending message into one specific chat or all cached chats.
  - **\<Message\>** => Message to send. Please use **""** to indicate message. Markdown formating allowed.
  - **-c | --chat** => Chat with according Chat Id. If not specified, message is sent to all chats.

Example: **send -c 123456 "Example message"** or **send "Hello World!"**

- **register \<Chat Id\>** => Manipulate registered chats:
  - **\<Chat Id\>** => Chat to (add to / remove from) the list.
  - **-l | --list** => Show the list of registered chats.
  - **-r | --remove** => Remove one specific chat from list.

Example: **register 123456** or **register -rm "-69420"**

- **leave / l \<Chat Id\>** => Leave a specific chat.
  - **\<Chat Id\>** => Chat that bot will leave.

Example: **leave 123456** or **l "-69420"**

- **reload / r**  => Reload configurations (See: [Configuration](TelegramWarnBot/Configuration/Configuration.json), [Triggers](TelegramWarnBot/Configuration/Triggers.json) and [IllegalTriggers](TelegramWarnBot/Configuration/IllegalTriggers.json)).

- **save / s** => Save last cached data to json files (See: [Users](TelegramWarnBot/Data/Users.json), [Chats](TelegramWarnBot/Data/Chats.json) and [ChatWarnings](TelegramWarnBot/Data/ChatWarnings.json)).

- **info / i** => Show info about cached chats, users and warnings.

- **version / v** => Version of bot.

## Like the project?

Give it a :star: Star!

## Found a bug?

Drop to <a href="https://github.com/Geras1mleo/TelegramWarnBot/issues">Issues</a><br/>
Or: sviatoslav.harasymchuk@gmail.com<br/>
<br/>
Thanks in advance!
