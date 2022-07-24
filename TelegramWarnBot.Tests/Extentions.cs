namespace TelegramWarnBot.Tests;

public static class Extentions
{
    public static Update BuildMessageUpdate(this IFixture fixture, string message, long chatId = 69, long userId = 420)
    {
        return fixture.Build<Update>()
                            .With(u => u.Message, fixture.BuildMessage(message, chatId, userId))
                            .Create();
    }

    public static Message BuildMessage(this IFixture fixture, string message, long chatId, long userId)
    {
        return fixture.Build<Message>()
                      .With(m => m.Text, message)
                      .With(m => m.Chat, fixture.BuildChat(chatId))
                      .With(m => m.From, fixture.BuildUser(userId))
                      .Create();
    }

    public static Chat BuildChat(this IFixture fixture, long chatId)
    {
        return fixture.Build<Chat>()
                      .With(c => c.Id, chatId)
                      .Create();
    }

    public static User BuildUser(this IFixture fixture, long userId)
    {
        return fixture.Build<User>()
                      .With(u => u.Id, userId)
                      .Create();
    }
}
