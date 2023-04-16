using ChatService.Web.Dtos;
using ChatService.Web.Exceptions;
using ChatService.Web.Services;
using ChatService.Web.Storage;
using Moq;


namespace ChatService.Web.Test;







    public class ConversationServiceTest
    {
    private readonly Mock<IConversationStore> _ConversationStoreMock = new();
    private readonly Mock<IProfileStore> _ProfileStoreMock = new();
    private readonly Mock<IMessageStore> _MessageStoreMock = new();
    private readonly ConversationService _conversationService;
    private readonly Profile _profile;
    public ConversationServiceTest()
    {
        _conversationService = new ConversationService(_ConversationStoreMock.Object, _MessageStoreMock.Object, _ProfileStoreMock.Object);
        _profile = new Profile("foobar", "Foo", "Bar", "imgid");
    }


    [Fact]
    public async Task StartConversation_Should_Return_StartConversationResponse()
    {

        var participants = new List<string> { "user1", "user2" };
        var firstMessage = new SendMessageDto("messageid", "senderid", "Hello");
        var conversationId = "user1_user2";

        var userConversation = new UserConversation
        (
            Username: participants[0],
            ConversationId: conversationId,
            Participant: participants[1],
            LastModifiedUnixTime: 0
        );


        

        _ProfileStoreMock.Setup(mock => mock.GetProfile(It.IsAny<string>())).ReturnsAsync(_profile);

        _MessageStoreMock.Setup(mock => mock.CreateMessage(It.IsAny<SendMessageDto>(), It.IsAny<string>())).ReturnsAsync(1231424);

        _ConversationStoreMock.Setup(mock => mock.CreateUserConversation(It.IsAny<UserConversation>())).ReturnsAsync(1231424);



        var result = await _conversationService.StartConversation(participants, firstMessage);

        Assert.NotNull(result);
        Assert.Equal(conversationId, result.Id);


    }


    //[Fact]
    //public async Task StartConversation_Should_Throw_HttpException_When_FirstMessageSender_Does_Not_Exist()
    //{
    //    // Arrange
    //    var participants = new List<string> { "user1", "user2" };
    //    var firstMessage = new SendMessageDto("messageid", "senderid", "Hello");

    //    _ProfileStoreMock.Setup(mock => mock.GetProfile(It.IsAny<string>())).ReturnsAsync((Profile)null);

    //    var conversationService = new ConversationService(_ConversationStoreMock.Object, _MessageStoreMock.Object, _ProfileStoreMock.Object);
    //    await Assert.ThrowsAsync<HttpException>(async () =>
    //    {
    //        await _conversationService.StartConversation(participants, firstMessage);
    //    });
    //}

    //[Fact]
    //public async Task StartConversation_Should_Throw_HttpException_When_Participants_Do_Not_Exist()
    //{
    //    // Arrange
    //    var participants = new List<string> { "user1", "user2" };
    //    var firstMessage = new SendMessageDto("messageid", "senderid", "Hello");

    //    _ProfileStoreMock.SetupSequence(mock => mock.GetProfile(It.IsAny<string>()))
    //        .ReturnsAsync(_profile)
    //        .ReturnsAsync((Profile)null);

    //    // Act and Assert
    //    await Assert.ThrowsAsync<HttpException>(async () =>
    //    {
    //        await _conversationService.StartConversation(participants, firstMessage);
    //    });
    //}





    [Fact]
    public async Task GetUserConversations_ReturnsCorrectResponse()
    {
        // Arrange
        var username = "username";
        var continuationToken = "continuationToken";
        var limit = 10;
        var lastSeenMessageTime = 123456789;
        var getUserConversationsResponse = new GetUserConversationDto
        (
            UserConversations: new List<UserConversation>()
            {
            new UserConversation
            (
                Username: "anyusername",
                ConversationId: "conversationId1",
                LastModifiedUnixTime: 123456789,
                Participant: "participant1"
            )
            },
            ContinuationToken: "newContinuationToken"
        );
        _ConversationStoreMock.Setup(x => x.GetUserConversations(username, continuationToken, limit, lastSeenMessageTime)).ReturnsAsync(getUserConversationsResponse);

        var recipient1 = new Profile("participant1", "Recipient1", "Profile1", "imgid1");
        var recipient2 = new Profile("participant2", "Recipient2", "Profile2", "imgid2");
        _ProfileStoreMock.Setup(x => x.GetProfile("participant1")).ReturnsAsync(recipient1);
        _ProfileStoreMock.Setup(x => x.GetProfile("participant2")).ReturnsAsync(recipient2);

        // Act
        var result = await _conversationService.GetUserConversations(username, continuationToken, limit, lastSeenMessageTime);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(getUserConversationsResponse.ContinuationToken, result.ContinuationToken);
        Assert.Equal(getUserConversationsResponse.UserConversations.Count, result.ConversationsInfo.Count);

        Assert.Equal(getUserConversationsResponse.UserConversations[0].ConversationId, result.ConversationsInfo[0].Id);
        Assert.Equal(getUserConversationsResponse.UserConversations[0].LastModifiedUnixTime, result.ConversationsInfo[0].lastModifiedUnixTime);
        Assert.Equal(recipient1, result.ConversationsInfo[0].recipient);
    }






    [Fact]
    public async Task UpdateUserConversation_Should_Return_CreatedUnixTime()
    {
        // Arrange
        var userConversation = new UserConversation
        (
            Username: "username",
            ConversationId: "conversationId",
            LastModifiedUnixTime: 0,
            Participant: "participant"
        );

        _ConversationStoreMock.Setup(mock => mock.UpsertUserConversation(It.IsAny<UserConversation>())).ReturnsAsync(123456789);

        // Act
        var result = await _conversationService.UpdateUserConversation(userConversation);

        // Assert
        Assert.Equal(123456789, result);
    }

    [Fact]
    public async Task UpdateUserConversation_Should_Throw_Exception_When_Failed()
    {
        // Arrange
        var userConversation = new UserConversation
        (
            Username: "username",
            ConversationId: "conversationId",
            LastModifiedUnixTime: 0,
            Participant: "participant"
        );

        _ConversationStoreMock.Setup(mock => mock.UpsertUserConversation(It.IsAny<UserConversation>())).ThrowsAsync(new Exception("Failed to update user conversation."));

        // Act and Assert
        await Assert.ThrowsAsync<Exception>(async () =>
        {
            await _conversationService.UpdateUserConversation(userConversation);
        });
    }






























}

















