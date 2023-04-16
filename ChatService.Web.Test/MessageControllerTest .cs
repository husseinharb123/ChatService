using System.Net;
using System.Net.Http.Json;
using System.Text;
using ChatService.Web.Dtos;
using ChatService.Web.Exceptions;
using ChatService.Web.Services;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Newtonsoft.Json;

namespace ChatService.Web.Test;

public class MessageControllerTest : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly Mock<IMessageService> _MessageServiceMock = new();
    private readonly Mock<IConversationService> _ConversationServiceMock = new();
    private readonly HttpClient _httpClient;
    private readonly SendMessageDto _message;

    public MessageControllerTest(WebApplicationFactory<Program> factory)
    {
        _httpClient = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services => { services.AddSingleton(_MessageServiceMock.Object); services.AddSingleton(_ConversationServiceMock.Object); });
        }).CreateClient();


        _message = new SendMessageDto(
            Id: "anyid",
            SenderUsername: "anyid",
            Text : "anymessage"

        );

    }



    [Fact]
    public async Task SendMessageToConversation_ValidRequest_ReturnsCreated()
    {
        // Arrange
        var conversationId = "conversationId";
        _MessageServiceMock.Setup(x => x.SendMessage(It.IsAny<string>(), It.IsAny<SendMessageDto>())).ReturnsAsync(134353535);

        // Act
        var result = await _httpClient.PostAsJsonAsync($"/api/conversations/{conversationId}/messages", _message);

        // Assert
        Assert.Equal(HttpStatusCode.Created, result.StatusCode);
        _MessageServiceMock.Verify(x => x.SendMessage(conversationId, _message), Times.Once);
    }

    [Fact]
    public async Task SendMessageToConversation_NullMessage_ReturnsBadRequest()
    {
        // Arrange
        var conversationId = "conversationId";
        var stringContent = new StringContent("", Encoding.UTF8, "application/json");

        // Act
        var result = await _httpClient.PostAsync($"/api/conversations/{conversationId}/messages", stringContent);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
    }

    [Fact]
    public async Task SendMessageToConversation_ConflictException_ReturnsConflict()
    {
        // Arrange
        var conversationId = "conversationId";
        var exceptionMessage = "Conflict occurred.";
        _MessageServiceMock.Setup(x => x.SendMessage(It.IsAny<string>(), It.IsAny<SendMessageDto>())).ThrowsAsync(new HttpException(exceptionMessage, 409));

        // Act
        var result = await _httpClient.PostAsJsonAsync($"/api/conversations/{conversationId}/messages", _message);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, result.StatusCode);
        var content = await result.Content.ReadAsStringAsync();
        Assert.Equal(exceptionMessage, content);
    }

    //[Fact]
    //public async Task SendMessageToConversation_NotFoundException_ReturnsNotFound()
    //{
    //    // Arrange
    //    var conversationId = "conversationId";
    //    var exceptionMessage = "Conversation not found.";
    //    _MessageServiceMock.Setup(x => x.SendMessage(It.IsAny<string>(), It.IsAny<SendMessageDto>())).ThrowsAsync(new HttpException(exceptionMessage, 404));

    //    // Act
    //    var result = await _httpClient.PostAsJsonAsync($"/api/conversation/{conversationId}/messages", _message);

    //    // Assert
    //    Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
    //    var content = await result.Content.ReadAsStringAsync();
    //    Assert.Equal(exceptionMessage, content);
    //}


    [Fact]
    public async Task SendMessageToConversation_GenericException_ReturnsInternalServerError()
    {
        var conversationId = "conversationId";
        var exceptionMessage = "Internal server error.";
        _MessageServiceMock.Setup(x => x.SendMessage(It.IsAny<string>(), It.IsAny<SendMessageDto>())).ThrowsAsync(new Exception(exceptionMessage));


        var result = await _httpClient.PostAsJsonAsync($"/api/conversations/{conversationId}/messages", _message);

        Assert.Equal(HttpStatusCode.InternalServerError, result.StatusCode);
        var content = await result.Content.ReadAsStringAsync();
        Assert.Equal(exceptionMessage, content);
    }









[Fact]
public async Task SendMessageToConversation_ReturnsCreatedResult()
{
    // Arrange
    var conversationId = "conversationId";
    _MessageServiceMock.Setup(x => x.SendMessage(conversationId, _message)).ReturnsAsync(123);

        
        var response = await _httpClient.PostAsJsonAsync($"/api/conversations/{conversationId}/messages", _message);

    // Assert
    Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    var responseData = await response.Content.ReadFromJsonAsync<SendMessageResponse>();
    Assert.NotNull(responseData);
    _MessageServiceMock.Verify(x => x.SendMessage(conversationId, _message), Times.Once);
}

[Fact]
public async Task SendMessageToConversation_ReturnsBadRequestResult_WhenMessageIsNull()
{
    // Arrange
    SendMessageDto message = null;
        var conversationId = "conversationId";
        var response = await _httpClient.PostAsJsonAsync($"/api/conversations/{conversationId}/messages", message);
        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    _MessageServiceMock.Verify(x => x.SendMessage(It.IsAny<string>(), It.IsAny<SendMessageDto>()), Times.Never);
}

[Fact]
public async Task SendMessageToConversation_ReturnsConflictResult_WhenHttpExceptionWithStatusCode409IsThrown()
{
    // Arrange
    var conversationId = "conversationId";
    var exceptionMessage = "Conflict occurred.";
    _MessageServiceMock.Setup(x => x.SendMessage(conversationId, _message)).ThrowsAsync(new HttpException(exceptionMessage,409 ));

        // Act
        var response = await _httpClient.PostAsJsonAsync($"/api/conversations/{conversationId}/messages", _message);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    Assert.Equal(exceptionMessage, await response.Content.ReadAsStringAsync());
    _MessageServiceMock.Verify(x => x.SendMessage(conversationId, _message), Times.Once);
}



[Fact]
public async Task SendMessageToConversation_ReturnsInternalServerErrorResult_WhenExceptionIsThrown()
{
    // Arrange
    var conversationId = "conversationId";
    var exceptionMessage = "Internal server error.";
    _MessageServiceMock.Setup(x => x.SendMessage(conversationId, _message)).ThrowsAsync(new Exception(exceptionMessage));

        // Act
        var response = await _httpClient.PostAsJsonAsync($"/api/conversations/{conversationId}/messages", _message);

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    _MessageServiceMock.Verify(x => x.SendMessage(conversationId, _message), Times.Once);
}





//[Fact]
//public async Task SendMessageToConversation_ReturnsNotFoundResult_WhenHttpExceptionWithStatusCode404IsThrown()
//{
//    // Arrange
//    var conversationId = "conversationId";
//    var exceptionMessage = "Conversation not found.";
//    _MessageServiceMock.Setup(x => x.SendMessage(conversationId, _message)).ThrowsAsync(new HttpException( exceptionMessage,404));

//        // Act
//        var response = await _httpClient.PostAsJsonAsync($"/api/conversations/{conversationId}/messages", _message);

//        // Assert
//        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
//    Assert.Equal(exceptionMessage, await response.Content.ReadAsStringAsync());


//}








    [Theory]
    [InlineData(null)]
    [InlineData("anytoken")]
    public async Task GetMessagesOfConversation_ReturnsOkResult_WithMessagesAndNextUri(string lastContinuationToken)
    {
        // Arrange
        var conversationId = "conversation1";
        var continuationToken = "token1";
        var limit = 10;
        var lastSeenMessageTime = 0;
        var responseDto = new GetConversationMessageServiceResponse
        (
            Messages : new List<GetMessageDto>
            {                     
                new GetMessageDto (SenderUsername : "anyid", Text : "Hello" ,UnixTime:0),

                },
            ContinuationToken: lastContinuationToken
       );
        _MessageServiceMock.Setup(mock => mock.GetConversationMessages(conversationId, continuationToken, limit, lastSeenMessageTime))
            .ReturnsAsync(responseDto);

        string expectednexturi = null;

        if (lastContinuationToken != null) {
            expectednexturi = $"/api/conversations/{conversationId}/messages?&limit={limit}&lastSeenMessageTime={lastSeenMessageTime}&continuationToken={lastContinuationToken}";
        }


        var endpoint = $"/api/conversations/{conversationId}/messages?continuationToken={continuationToken}&limit={limit}&lastSeenMessageTime={lastSeenMessageTime}";
        var response = await _httpClient.GetAsync(endpoint);
        var content = await response.Content.ReadAsStringAsync();
        var getConversationsOfUserResponse = JsonConvert.DeserializeObject<GetConversationsOfUserResponse>(content);
        
        
        Assert.Equal(expectednexturi, getConversationsOfUserResponse.NextUri);
        Assert.Equal(expectednexturi, getConversationsOfUserResponse.NextUri);
    }

    //[Fact]
    //public async Task GetMessagesOfConversation_ReturnsNotFoundResult_WhenHttpExceptionWithStatusCode404IsThrown()
    //{
    //    // Arrange
    //    var conversationId = "conversation1";
    //    var continuationToken = "token1";
    //    var limit = 10;
    //    var lastSeenMessageTime = 0;
    //    _MessageServiceMock.Setup(mock => mock.GetConversationMessages(conversationId, continuationToken, limit, lastSeenMessageTime))
    //        .ThrowsAsync(new HttpException("Conversation not found", 404));

    //    // Act
    //    var response = await _httpClient.GetAsync($"/api/conversations/{conversationId}/messages?continuationToken={continuationToken}&limit={limit}&lastSeenMessageTime={lastSeenMessageTime}");

    //    // Assert
    //    Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    //    var responseBody = await response.Content.ReadAsStringAsync();
    //    Assert.Contains("Conversation not found", responseBody);
    //}



    [Fact]
    public async Task GetMessagesOfConversation_ReturnsInternalServerError()
    {
        // Arrange
        var conversationId = "conversation1";
        var continuationToken = "token1";
        var limit = 10;
        var lastSeenMessageTime = 0;
        _MessageServiceMock.Setup(mock => mock.GetConversationMessages(conversationId, continuationToken, limit, lastSeenMessageTime))
            .ThrowsAsync(new Exception());

        // Act
        var response = await _httpClient.GetAsync($"/api/conversations/{conversationId}/messages?continuationToken={continuationToken}&limit={limit}&lastSeenMessageTime={lastSeenMessageTime}");

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);

    }

























}
































