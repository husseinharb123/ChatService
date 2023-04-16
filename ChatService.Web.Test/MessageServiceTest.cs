using ChatService.Web.Dtos;
using ChatService.Web.Exceptions;
using ChatService.Web.Services;
using ChatService.Web.Storage;
using Moq;


namespace ChatService.Web.Test;







public class MessageServiceTest
{
    private readonly Mock<IConversationStore> _ConversationStoreMock = new();
    private readonly Mock<IProfileStore> _ProfileStoreMock = new();
    private readonly Mock<IMessageStore> _MessageStoreMock = new();
    private readonly MessageService _messageService;
    private readonly Profile _profile;
    public MessageServiceTest()
    {
        _messageService = new MessageService(_ConversationStoreMock.Object, _MessageStoreMock.Object, _ProfileStoreMock.Object);
        _profile = new Profile("foobar", "Foo", "Bar", "imgid");
    }




    [Fact]
    public async Task GetConversationMessages_Should_Return_ConversationMessageResponse_With_Messages_And_ContinuationToken()
    {
        // Arrange
        var conversationId = "conversationId";
        var continuationToken = "continuationToken";
        var limit = 10;
        var lastSeenMessageTime = 123456789;



        var message1 = new Message
        (   Id: "anyid" ,
            Text : "Hello",
            SenderId : "sender1",
            CreatedUnixTime : 123456789,
            ConversationId : conversationId
        );
        var message2 = new Message
        (Id: "anyid",
            Text: "Hi",
            SenderId: "sender1",
            CreatedUnixTime: 123456789,
            ConversationId: conversationId
        );
        var messages = new List<Message> { message1, message2 };
        var lastContinuationToken = "lastContinuationToken";

        _MessageStoreMock.Setup(mock => mock.GetConersationMessages(conversationId, continuationToken, limit, lastSeenMessageTime))
            .ReturnsAsync(new GetConverationMessagesDto(messages, lastContinuationToken));

        // Act
        var result = await _messageService.GetConversationMessages(conversationId, continuationToken, limit, lastSeenMessageTime);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Messages.Count);
        Assert.Equal("Hello", result.Messages[0].Text);
        Assert.Equal("Hi", result.Messages[1].Text);
        Assert.Equal(lastContinuationToken, result.ContinuationToken);
    }

    [Fact]
    public async Task GetConversationMessages_Should_Throw_Exception_When_MessageStore_Fails()
    {
        // Arrange
        var conversationId = "conversationId";
        var continuationToken = "continuationToken";
        var limit = 10;
        var lastSeenMessageTime = 123456789;

        _MessageStoreMock. Setup(mock => mock.GetConersationMessages(conversationId, continuationToken, limit, lastSeenMessageTime))
            .ThrowsAsync(new Exception("Failed to get conversation messages."));

        // Act and Assert
        await Assert.ThrowsAsync<Exception>(async () =>
        {
            await _messageService.GetConversationMessages(conversationId, continuationToken, limit, lastSeenMessageTime);
        });
    }




    [Fact]
    public async Task SendMessage_Should_Return_Created_MessageId_When_Sender_Exists_And_Message_Created_Successfully()
    {
        // Arrange
        var conversationId = "conversationId";
        var senderUsername = "senderUsername";
        var participants = new List<string> { "user1", "user2" };
        var message = new SendMessageDto
       (
            Id : "anyid",
            SenderUsername: senderUsername,
            Text: "Hello"
        );
        var userConversation = new UserConversation
        (
            Username: participants[0],
            ConversationId: conversationId,
            Participant: participants[1],
            LastModifiedUnixTime: 0
        );
        var senderProfile = _profile;

        var messageId = 12345;

        _ConversationStoreMock.Setup(mock => mock.GetUserConversation(conversationId, senderUsername))
            .ReturnsAsync(userConversation);
        _ProfileStoreMock.Setup(mock => mock.GetProfile(senderUsername))
            .ReturnsAsync(senderProfile);
        _MessageStoreMock.Setup(mock => mock.CreateMessage(message, conversationId))
            .ReturnsAsync(messageId);
        _ConversationStoreMock.Setup(mock => mock.UpsertUserConversation(userConversation))
            .ReturnsAsync(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());

        // Act
        var result = await _messageService.SendMessage(conversationId, message);

        // Assert
        Assert.Equal(messageId, result);
    }

    [Fact]
    public async Task SendMessage_Should_Throw_HttpException_With_404_Status_When_Conversation_Not_Found()
    {
        // Arrange
        var conversationId = "conversationId";
        var senderUsername = "senderUsername";
        var participants = new List<string> { "user1", "user2" };
        var message = new SendMessageDto
       (
            Id: "anyid",
            SenderUsername: senderUsername,
            Text: "Hello"
        );

        _ConversationStoreMock.Setup(mock => mock.GetUserConversation(conversationId, senderUsername))
            .ReturnsAsync((UserConversation)null);

        // Act and Assert
        await Assert.ThrowsAsync<HttpException>(async () =>
        {
            await _messageService.SendMessage(conversationId, message);
        });

        _ConversationStoreMock.Verify(mock => mock.GetUserConversation(conversationId, senderUsername), Times.Once);
        _ProfileStoreMock.Verify(mock => mock.GetProfile(It.IsAny<string>()), Times.Never);
        _MessageStoreMock.Verify(mock => mock.CreateMessage(It.IsAny<SendMessageDto>(), It.IsAny<string>()), Times.Never);
        _ConversationStoreMock.Verify(mock => mock.UpsertUserConversation(It.IsAny<UserConversation>()), Times.Never);
    }
















}



