namespace TelegramWarnBot.Tests;

public class WarnSuccessIntegrationTests : IntegrationTestsBase
{
    [Fact]
    public async Task AdminWarnsMember3Times_MemberGetsBanned()
    {
        // Arrange
        string inputMention = @"{
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

        // Arrange
        string inputReply = @"{
        ""update_id"":0,
        ""message"": {
                ""message_id"":0,
                ""from"":{""id"":654,""is_bot"":false,""first_name"":""Admin"",""last_name"":null,""username"":""admin_of_the_chat""},
                ""chat"":{""id"":69,""title"":""Bot test"",""type"":""supergroup""},
                ""date"":1659438893,
                ""text"":""/warn"",
                ""entities"":[
                    {""offset"":0,""length"":5,""type"":""bot_command""},
                ],
                ""reply_to_message"": {
                    ""message_id"":2,
                    ""date"":1659452587,
                    ""from"":{ ""id"":420,""is_bot"":false,""first_name"":""Robert"",""last_name"":""Johnson"",""username"":""robert_johnson""},
                    ""chat"":{""id"":69,""title"":""Bot test"",""type"":""supergroup""},
                }
            }
        }";


        var updateMention = JsonConvert.DeserializeObject<Update>(inputMention);
        var updateReply = JsonConvert.DeserializeObject<Update>(inputReply);

        // Act
        await bot.UpdateHandler(null, updateReply, CancellationToken.None);
        await bot.UpdateHandler(null, updateReply, CancellationToken.None);
        await bot.UpdateHandler(null, updateMention, CancellationToken.None);

        // Assert
        await clientProvider.Received(2).SendMessageAsync(Arg.Is<ChatId>(69), Arg.Is("WarnedSuccessfully"));
        await clientProvider.Received(1).SendMessageAsync(Arg.Is<ChatId>(69), Arg.Is("BannedSuccessfully"));

        await clientProvider.Received(3).DeleteMessageAsync(Arg.Is<ChatId>(69), Arg.Is(0)); // From config delete "/warn" messages
        await clientProvider.Received(1).BanChatMemberAsync(Arg.Is<ChatId>(69), Arg.Is<long>(420)); // Robert Jonson gets banned
    }
}
