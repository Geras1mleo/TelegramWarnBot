using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace TelegramWarnBot.Tests;

public class IntegrationTests
{
    private readonly ITelegramBotClientProvider clientProvider = Substitute.For<ITelegramBotClientProvider>();
    private readonly IBot bot;

    public IntegrationTests()
    {
        // todo report issue
        //client.DeleteMessageAsync(default, default).ReturnsForAnyArgs(Task.CompletedTask);
        //client.BanChatMemberAsync(default, default).ReturnsForAnyArgs(Task.CompletedTask);
        //client.UnbanChatMemberAsync(default, default).ReturnsForAnyArgs(Task.CompletedTask);

        clientProvider.SendMessageAsync(default, default).ReturnsForAnyArgs(Task.FromResult(new Message()));
        clientProvider.DeleteMessageAsync(default, default).ReturnsForAnyArgs(Task.CompletedTask);
        clientProvider.BanChatMemberAsync(default, default).ReturnsForAnyArgs(Task.CompletedTask);
        clientProvider.UnbanChatMemberAsync(default, default).ReturnsForAnyArgs(Task.CompletedTask);
        clientProvider.GetChatAdministratorsAsync(default).ReturnsForAnyArgs(Task.FromResult(new ChatMember[] { new ChatMemberMember() }));
        clientProvider.ForwardMessageAsync(default, default, default).ReturnsForAnyArgs(Task.FromResult(new Message()));
        clientProvider.GetMeAsync().ReturnsForAnyArgs(Task.FromResult(new User() { Id = 99, Username = "warn_bot", FirstName = "Moderator" }));

        var servicesCollection = new ServiceCollection()
            .AddSingleton(clientProvider)
            .AddSingleton<IBot, Bot>()
            .AddSingleton<IConfigurationContext>(MockedConfigurationContext.Shared)
            .AddSingleton<ICachedDataContext>(MockedCachedContext.Shared)
            .AddSingleton<IInMemoryCachedDataContext>(MockedInMemoryCachedContext.Shared)
            .AddTransient<IUpdateContextBuilder, UpdateContextBuilder>()
            .AddTransient<IDateTimeProvider, DateTimeProvider>()
            .AddTransient<ICommandsController, CommandsController>()
            .AddTransient<IMessageHelper, MessageHelper>()
            .AddTransient<IChatHelper, ChatHelper>()
            .AddTransient<ICommandService, CommandService>()
            .AddTransient<IResponseHelper, ResponseHelper>()
            .AddSmartFormatterProvider()
            .AddSingleton(Substitute.For<ILogger<Bot>>())
            .AddSingleton(Substitute.For<ILogger<AdminsHandler>>())
            .AddSingleton(Substitute.For<ILogger<CommandHandler>>())
            .AddSingleton(Substitute.For<ILogger<IllegalTriggersHandler>>())
            .AddSingleton(Substitute.For<ILogger<JoinedLeftHandler>>())
            .AddSingleton(Substitute.For<ILogger<SpamHandler>>())
            .AddSingleton(Substitute.For<ILogger<TriggersHandler>>())
            .AddSingleton(Substitute.For<ILogger<CommandsController>>());

        var services = servicesCollection.BuildServiceProvider();

        bot = services.GetService<IBot>();
        bot.StartAsync(CancellationToken.None).GetAwaiter().GetResult();
    }

    [Fact]
    public async Task AdminWarnsMember3Times_MemberGetsBanned()
    {
        // Arrange
        string input = @"{
        ""update_id"":0,
        ""message"": {
                ""message_id"":0,
                ""from"":{""id"":654,""is_bot"":false,""first_name"":""Admin"",""last_name"":null,""username"":""admin_of_the_chat""},
                ""chat"":{""id"":69,""title"":""Bot test"",""type"":""supergroup""},
                ""date"":1659438893,
                ""text"":""/warn @robert_johnson"",
                ""entities"":[
                    {""offset"":0,""length"":5,""type"":""bot_command""},
                    {""offset"":6,""length"":15,""type"":""mention""}
                ]
            }
        }";

        var update = JsonConvert.DeserializeObject<Update>(input);

        // Act
        await bot.UpdateHandler(null, update, CancellationToken.None);
        await bot.UpdateHandler(null, update, CancellationToken.None);
        await bot.UpdateHandler(null, update, CancellationToken.None);

        // Assert
        await clientProvider.ReceivedWithAnyArgs(3).SendMessageAsync(default, default);
        await clientProvider.ReceivedWithAnyArgs(3).DeleteMessageAsync(default, default); // From config delete "/warn" messages
        await clientProvider.ReceivedWithAnyArgs(1).BanChatMemberAsync(default, default);
    }
}
