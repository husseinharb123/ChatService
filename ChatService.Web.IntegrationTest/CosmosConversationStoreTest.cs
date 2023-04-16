using ChatService.Web.Dtos;
using ChatService.Web.Exceptions;
using ChatService.Web.Storage;
using ChatService.Web.Storage.Entities;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;
using System.Runtime.InteropServices;

namespace ChatService.Web.IntegrationTest;

public class CosmosConversationStoreTest : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly IMessageStore _Messagestore;

    private readonly IConversationStore _conversationStore;
    private readonly IProfileStore _Profilestore;
    private readonly Profile _profile;
    private readonly SendMessageDto _sendMessageDto;


    public CosmosConversationStoreTest(WebApplicationFactory<Program> factory)
    {
        _Messagestore = factory.Services.GetRequiredService<IMessageStore>();
        _Profilestore = factory.Services.GetRequiredService<IProfileStore>();
        _conversationStore = factory.Services.GetRequiredService<IConversationStore>();




        _sendMessageDto = new SendMessageDto(

                Id: Guid.NewGuid().ToString(),
                SenderUsername: "husseinharb",
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
    public async Task UpsertUserConversation_Should_Upsert_UserConversations()
    {
        var userConversation = new UserConversation(
            Username : "anyusername", 
            ConversationId : Guid.NewGuid().ToString(),
            Participant : "anyparticpant",
            LastModifiedUnixTime : 100 
            
            
            );

        var upsertResponse = await _conversationStore.UpsertUserConversation(userConversation);

        Assert.NotNull(upsertResponse);
        Assert.IsType<long>(upsertResponse);
    
    
    }


    [Fact]
    public async Task CreateUserConversation_Should_Create_UserConversations()
    {
        var userConversation = new UserConversation(
            Username: "anyusername",
            ConversationId: Guid.NewGuid().ToString(),
            Participant: "anyparticpant",
            LastModifiedUnixTime: 100


            );

        var CreateUserResponse = await _conversationStore.CreateUserConversation(userConversation);

        Assert.NotNull(CreateUserResponse);
        Assert.IsType<long>(CreateUserResponse);


    }




    [Fact]
    public async Task CreateUserConversation_Should_Return_Conlfict()
    {
        var userConversation = new UserConversation(
            Username: "anyusername",
            ConversationId: Guid.NewGuid().ToString(),
            Participant: "anyparticpant",
            LastModifiedUnixTime: 100


            );

        var CreatetUser1Response = await _conversationStore.CreateUserConversation(userConversation);
        var ex = await Assert.ThrowsAsync<HttpException>(async () => await _conversationStore.CreateUserConversation(userConversation));

        Assert.Equal(409, ex.StatusCode);



    }








    [Fact]
    public async Task GetUserConversation_ExistingConversation_ReturnsUserConversation()
    {
        // Arrange
        var conversationId = Guid.NewGuid().ToString(); 
        var username = "username2323232"; 
       
        var userConversation = new UserConversation(
        Username: username,
        ConversationId: conversationId,
        Participant: "anyparticpant",
        LastModifiedUnixTime: 100


    );
        var CreatetUserResponse = await _conversationStore.CreateUserConversation(userConversation);
        var userConversationResponse = await _conversationStore.GetUserConversation(conversationId, username);


        Assert.NotNull(userConversation);
        Assert.Equal(username, userConversationResponse.Username);

    }

    [Fact]
    public async Task GetUserConversation_NonExistingConversation_ReturnsNull()
    {
     
        var conversationId = "nonExistingConversationId"; 
        var username = "username"; 


        var userConversation = await _conversationStore.GetUserConversation(conversationId, username);


        Assert.Null(userConversation);
    }











    [Fact]
    public async Task GetUserConversations_ValidInput_ReturnsGetUserConversationDto()
    {
        // Arrange
        // Arrange
        var conversationId = Guid.NewGuid().ToString(); ;
        var username = "username2323232";

        var userConversation = new UserConversation(
        Username: username,
        ConversationId: conversationId,
        Participant: "anyparticpant",
        LastModifiedUnixTime: 100


    );
        
        string continuationToken = null;
        var limit = 1;
        var lastSeenMessageTime =0;

        var CreatetUserResponse = await _conversationStore.CreateUserConversation(userConversation);
        var result = await _conversationStore.GetUserConversations(username, continuationToken, limit, lastSeenMessageTime);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<GetUserConversationDto>(result);
        Assert.Equal(username , result.UserConversations[0].Username);
    }

    [Fact]
    public async Task GetUserConversations_CosmosException_ThrowsException()
    {
        // Arrange
        var username = "testuser";
        string continuationToken = "";
        var limit = 10;
        var lastSeenMessageTime = 0;

        // Act and Assert
        await Assert.ThrowsAsync<Exception>(async () =>
        {
            await _conversationStore.GetUserConversations(username, continuationToken, limit, lastSeenMessageTime);
        });
    }
}

























