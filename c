[1mdiff --git a/TelegramWarnBot/Source/AppConfiguration.cs b/TelegramWarnBot/Source/AppConfiguration.cs[m
[1mindex db197aa..cd69177 100644[m
[1m--- a/TelegramWarnBot/Source/AppConfiguration.cs[m
[1m+++ b/TelegramWarnBot/Source/AppConfiguration.cs[m
[36m@@ -67,7 +67,7 @@[m [mpublic static class AppConfiguration[m
         services.AddTransient<ICommand, SaveCommand>();[m
         services.AddTransient<ICommand, VersionCommand>();[m
         services.AddTransient<ICommand, ClearCommand>();[m
[31m-[m
[32m+[m[41m        [m
         services.AddTransient<IUpdateContextBuilder, UpdateContextBuilder>();[m
 [m
         services.AddTransient<IDateTimeProvider, DateTimeProvider>();[m
[1mdiff --git a/TelegramWarnBot/Source/Bot.cs b/TelegramWarnBot/Source/Bot.cs[m
[1mindex f88f266..3eb9a58 100644[m
[1m--- a/TelegramWarnBot/Source/Bot.cs[m
[1m+++ b/TelegramWarnBot/Source/Bot.cs[m
[36m@@ -68,14 +68,15 @@[m [mpublic class Bot : IBot[m
             var context = updateContextBuilder.Build(update, BotUser, cancellationToken);[m
 [m
             if (!context.IsJoinedLeftUpdate && context.ChatDTO is null)[m
[31m-                throw new Exception("Message received from uncached chat!");[m
[32m+[m[32m                throw new Exception("Message from uncached chat!");[m
 [m
             return pipe(context);[m
         }[m
         catch (Exception exception)[m
         {[m
[32m+[m[32m            var chat = update.GetChat();[m
             // Update that raised exception will be saved in Logs.json (and sent to tech support in private messages)[m
[31m-            logger.LogError(exception, "Handler error on update {@update} in chat {chat}", update, update.GetChat()?.Title);[m
[32m+[m[32m            logger.LogError(exception, "Handler error on update {@update} in chat {chat}", update, ($"{chat?.Title}: {chat?.Id}"));[m
             return Task.CompletedTask;[m
         }[m
     }[m
[1mdiff --git a/TelegramWarnBot/Source/Console/Commands/Register.cs b/TelegramWarnBot/Source/Console/Commands/Register.cs[m
[1mindex f392d12..bb6f5d7 100644[m
[1m--- a/TelegramWarnBot/Source/Console/Commands/Register.cs[m
[1m+++ b/TelegramWarnBot/Source/Console/Commands/Register.cs[m
[36m@@ -95,6 +95,8 @@[m [mpublic class RegisterCommand : CommandLineApplication, ICommand[m
                 Tools.WriteColor("\t[" + chat.Name + "]: " + chat.Id, ConsoleColor.Red, false);[m
             }[m
 [m
[32m+[m[32m            Console.WriteLine();[m
[32m+[m
             return 1;[m
         }[m
 [m
[1mdiff --git a/TelegramWarnBot/Source/Logging/TelegramSink.cs b/TelegramWarnBot/Source/Logging/TelegramSink.cs[m
[1mindex 7550139..bb36657 100644[m
[1m--- a/TelegramWarnBot/Source/Logging/TelegramSink.cs[m
[1m+++ b/TelegramWarnBot/Source/Logging/TelegramSink.cs[m
[36m@@ -14,9 +14,13 @@[m [mpublic class TelegramSink : ILogEventSink[m
         if (notifyBotOwners is null)[m
             return;[m
 [m
[32m+[m[32m        logEvent.Properties.TryGetValue("update", out var property);[m
[32m+[m
         logEvent.RemovePropertyIfPresent("update");[m
 [m
[31m-        string message = logEvent.RenderMessage() + logEvent.Exception?.Message;[m
[32m+[m[32m        string message = logEvent.RenderMessage().Replace(" {@update}", "") + "\n" + logEvent.Exception?.Message;[m
[32m+[m
[32m+[m[32m        logEvent.AddPropertyIfAbsent(new LogEventProperty("update", property));[m
 [m
         try[m
         {[m
