namespace TelegramWarnBot.Tests;

public class UnwarnNotAllowedIntegrationTests : IntegrationTestsBase
{
    [Fact]
    public async Task AdminUnwarnsBot_NotAllowed()
    {
        // Arrange
        string input = @"{
        ""update_id"":0,
        ""message"": {
                ""message_id"":0,
                ""from"":{""id"":654,""is_bot"":false,""first_name"":""Admin"",""last_name"":null,""username"":""admin_of_the_chat""},
                ""chat"":{""id"":69,""title"":""Bot test"",""type"":""supergroup""},
                ""date"":1659438893,
                ""text"":""/unwarn"",
                ""entities"":[
                    {""offset"":0,""length"":7,""type"":""bot_command""},
                ],
                ""reply_to_message"": {
                    ""message_id"":2,
                    ""date"":1659452587,
                    ""from"":{ ""id"":98745,""is_bot"":true,""first_name"":""Some Bot"",""username"":""telegram_bot""},
                    ""chat"":{""id"":69,""title"":""Bot test"",""type"":""supergroup""},
                }
        }
        }";

        var update = JsonConvert.DeserializeObject<Update>(input);

        // Act
        await bot.UpdateHandler(null, update, CancellationToken.None);

        // Assert
        await clientProvider.Received(1).SendMessageAsync(Arg.Is<ChatId>(69), Arg.Is("UnwarnBotAttempt"));
        await clientProvider.ReceivedWithAnyArgs(0).DeleteMessageAsync(default, default);
    }

    [Fact]
    public async Task AdminUnwarnsBotItSelf_NotAllowed()
    {
        // Arrange
        string inputMention = @"{
        ""update_id"":0,
        ""message"": {
                ""message_id"":0,
                ""from"":{""id"":654,""is_bot"":false,""first_name"":""Admin"",""last_name"":null,""username"":""admin_of_the_chat""},
                ""chat"":{""id"":69,""title"":""Bot test"",""type"":""supergroup""},
                ""date"":1659438893,
                ""text"":""/unwarn @warn_bot"",
                ""entities"":[
                    {""offset"":0,""length"":7,""type"":""bot_command""},
                    {""offset"":8,""length"":9,""type"":""mention""}
                ]
        }
        }";

        string inputReply = @"{
        ""update_id"":0,
        ""message"": {
                ""message_id"":0,
                ""from"":{""id"":654,""is_bot"":false,""first_name"":""Admin"",""last_name"":null,""username"":""admin_of_the_chat""},
                ""chat"":{""id"":69,""title"":""Bot test"",""type"":""supergroup""},
                ""date"":1659438893,
                ""text"":""/unwarn"",
                ""entities"":[
                    {""offset"":0,""length"":7,""type"":""bot_command""},
                ],
                ""reply_to_message"": {
                    ""message_id"":2,
                    ""date"":1659452587,
                    ""from"":{ ""id"":99,""is_bot"":true,""first_name"":""Moderator"",""username"":""warn_bot""},
                    ""chat"":{""id"":69,""title"":""Bot test"",""type"":""supergroup""},
                }
        }
        }";

        var updateMention = JsonConvert.DeserializeObject<Update>(inputMention);
        var updateReply = JsonConvert.DeserializeObject<Update>(inputReply);

        // Act
        await bot.UpdateHandler(null, updateMention, CancellationToken.None);
        await bot.UpdateHandler(null, updateReply, CancellationToken.None);

        // Assert
        await clientProvider.Received(2).SendMessageAsync(Arg.Is<ChatId>(69), Arg.Is("UnwarnBotSelfAttempt"));
        await clientProvider.ReceivedWithAnyArgs(0).DeleteMessageAsync(default, default);
    }

    [Fact]
    public async Task AdminUnwarnsAnotherAdmin_NotAllowed()
    {
        mockedConfigurationContext.Configuration.AllowAdminWarnings = false;

        // Arrange
        string inputMention = @"{
        ""update_id"":0,
        ""message"": {
                ""message_id"":0,
                ""from"":{""id"":654,""is_bot"":false,""first_name"":""Admin"",""last_name"":null,""username"":""admin_of_the_chat""},
                ""chat"":{""id"":69,""title"":""Bot test"",""type"":""supergroup""},
                ""date"":1659438893,
                ""text"":""/unwarn @admin_of_the_chat"",
                ""entities"":[
                    {""offset"":0,""length"":7,""type"":""bot_command""},
                    {""offset"":8,""length"":18,""type"":""mention""}
                ]
        }
        }";

        string inputReply = @"{
        ""update_id"":0,
        ""message"": {
                ""message_id"":0,
                ""from"":{""id"":654,""is_bot"":false,""first_name"":""Admin"",""last_name"":null,""username"":""admin_of_the_chat""},
                ""chat"":{""id"":69,""title"":""Bot test"",""type"":""supergroup""},
                ""date"":1659438893,
                ""text"":""/unwarn"",
                ""entities"":[
                    {""offset"":0,""length"":7,""type"":""bot_command""},
                ],
                ""reply_to_message"": {
                    ""message_id"":2,
                    ""date"":1659452587,
                    ""from"":{ ""id"":654,""is_bot"":false,""first_name"":""Admin"",""username"":""admin_of_the_chat""},
                    ""chat"":{""id"":69,""title"":""Bot test"",""type"":""supergroup""},
                }
        }
        }";

        var updateMention = JsonConvert.DeserializeObject<Update>(inputMention);
        var updateReply = JsonConvert.DeserializeObject<Update>(inputReply);

        // Act
        await bot.UpdateHandler(null, updateMention, CancellationToken.None);
        await bot.UpdateHandler(null, updateReply, CancellationToken.None);

        // Assert
        await clientProvider.Received(2).SendMessageAsync(Arg.Is<ChatId>(69), Arg.Is("UnwarnAdminAttempt"));
        await clientProvider.ReceivedWithAnyArgs(0).DeleteMessageAsync(default, default);
    }
}