using ChatService.Web.Dtos;
using ChatService.Web.Exceptions;
using ChatService.Web.Storage;
using ChatService.Web.Storage.Entities;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;




namespace ChatService.Web.IntegrationTest;

    public class CosmosMessageStoreTest : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly IMessageStore _Messagestore;
    
    private readonly IConversationStore _conversationStore;
    private readonly IProfileStore _Profilestore;
    private readonly Profile _profile;
    private readonly SendMessageDto _sendMessageDto;


    public CosmosMessageStoreTest(WebApplicationFactory<Program> factory)
        {
            _Messagestore = factory.Services.GetRequiredService<IMessageStore>();
            _Profilestore = factory.Services.GetRequiredService<IProfileStore>();
            _conversationStore=factory.Services.GetRequiredService<IConversationStore>();




        _sendMessageDto = new SendMessageDto(
                
                Id : Guid.NewGuid().ToString(),
                SenderUsername : "husseinharb",
                Text: "Hello"
                
                );

            _profile = new(
            Username: Guid.NewGuid().ToString(),
            FirstName: "Foo",
            LastName: "Bar",
            ProfilePictureid: "djgksfksf"
        );


        }




    [Fact]
    public async Task CreateMessage_ShouldCreateMessage()
    {


        var messageid = Guid.NewGuid().ToString(); ;
        var sendMessageDto = new SendMessageDto(

            Id: messageid,
            SenderUsername: "husseinharb",
            Text: "Hello"

            );


        var conversationId = Guid.NewGuid().ToString(); 

        var CreateMessagRespond = await _Messagestore.CreateMessage(sendMessageDto, conversationId);
        Assert.NotNull(CreateMessagRespond);
        Assert.IsType<long>(CreateMessagRespond);


    }




    [Fact]
    public async Task CreateMessage_ShouldReturnConflict()
    {


        var messageid = Guid.NewGuid().ToString();
        var sendMessageDto = new SendMessageDto(

            Id: Guid.NewGuid().ToString(),
            SenderUsername: "husseinharb",
            Text: "Hello"

            );


        var conversationId = "anyconversationid1234090";

         await _Messagestore.CreateMessage(sendMessageDto, conversationId);
        var  ex = await Assert.ThrowsAsync<HttpException>(async () => await _Messagestore.CreateMessage(sendMessageDto, conversationId));

        Assert.Equal(409, ex.StatusCode);

    }








    [Fact]
    public async Task GetConersationMessages_WithValidParameters_ReturnsGetConverationMessagesDto()
    {

        string conversationId = Guid.NewGuid().ToString();
        var messageid = Guid.NewGuid().ToString();
        
        var sendMessageDto = new SendMessageDto(

            Id: messageid,
            SenderUsername: "husseinharb",
            Text: "Hello"

            );

        string continuationToken = null; 
        int limit = 1;
        long lastSeenMessageTime = 0;

        await _Messagestore.CreateMessage(sendMessageDto, conversationId);

        GetConverationMessagesDto result = await _Messagestore.GetConersationMessages(conversationId, continuationToken, limit, lastSeenMessageTime);
        var messages = result.Messages; 
        continuationToken = result.ContinuationToken;


        // Assert
        Assert.Equal(1, messages.Count);
        Assert.Equal(null, continuationToken);
        Assert.Equal(messageid, messages[0].Id);


    }

    //[Fact]
    //public async Task GetConersationMessages_WithNoMessagesFound_ThrowsHttpException()
    //{
    //    // Arrange
    //    string conversationId = "conversationId";
    //    string continuationToken = null;
    //    int limit = 10;
    //    long lastSeenMessageTime = 0;

    //    // Act & Assert
    //    var exception = await Assert.ThrowsAsync<HttpException>(() => _Messagestore.GetConersationMessages(conversationId, continuationToken, limit, lastSeenMessageTime));
    //    Assert.Equal(404, exception.StatusCode);
    //    Assert.Equal("No messages found for the given conversationId.", exception.Message);
    //}










}