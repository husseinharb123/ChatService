
using System;
using System.Net;
using System.Text;
using ChatService.Web.Controllers;
using ChatService.Web.Dtos;
using ChatService.Web.Exceptions;
using ChatService.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Newtonsoft.Json;

namespace ChatService.Web.Test

{ 
public class ConversationControllerTest : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly Mock<IConversationService> _ConversationServiceMock = new();
    private readonly HttpClient _httpClient;
    private readonly StartConversationRequest _startconversationRequest;

        public ConversationControllerTest(WebApplicationFactory<Program> factory)
    {
        _httpClient = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services => { services.AddSingleton(_ConversationServiceMock.Object); });
        }).CreateClient();


            _startconversationRequest = new StartConversationRequest
        (
            Participants: new List<string> { "participant1", "participant2" },
            FirstMessage: new SendMessageDto("id", "SenderUsername", " text")
        );




    }


    [Fact]
    public async Task StartConversation_ReturnsBadRequest_WhenRequestIsNull()
    {
        // Arrange
        StartConversationRequest request = null;
        var httpContent = new StringContent(JsonConvert.SerializeObject(request), Encoding.Default, "application/json");

        // Act
        var StartConversationResponse = await _httpClient.PostAsync($"/api/conversations/",httpContent);

            // Assert
            
        Assert.Equal(HttpStatusCode.BadRequest, StartConversationResponse.StatusCode);

    }


    [Theory]
    [InlineData(null, null)]
    public async Task StartConversation_ReturnsBadRequest_WhenRequestParametersAreInvalid(List<string> Participants, SendMessageDto FirstMessage)
    {
        // Arrange
        StartConversationRequest request = new StartConversationRequest(Participants,FirstMessage);
        var httpContent = new StringContent(JsonConvert.SerializeObject(request), Encoding.Default, "application/json");

        // Act
        var StartConversationResponse = await _httpClient.PostAsync($"/api/conversations/", httpContent);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, StartConversationResponse.StatusCode);

    }


   


    [Fact]
    public async Task StartConversation_ReturnsCreated_WhenConversationCreatedSuccessfully()
    {
        var ExpectedStartConversationResponse = new StartConversationResponse(Id: "1233232", CreatedUnixTime: 12341);

    
        _ConversationServiceMock.Setup(s => s.StartConversation( It.IsAny<List<string>>(), It.IsAny<SendMessageDto>()))
                   .ReturnsAsync(ExpectedStartConversationResponse);

        var httpContent = new StringContent(JsonConvert.SerializeObject(_startconversationRequest), Encoding.Default, "application/json");


        var StartConversationResponse = await _httpClient.PostAsync($"/api/conversations/", httpContent);

        Assert.Equal(HttpStatusCode.Created, StartConversationResponse.StatusCode);

        var jsonResponse = await StartConversationResponse.Content.ReadAsStringAsync();

        Assert.Equal(ExpectedStartConversationResponse, JsonConvert.DeserializeObject<StartConversationResponse>(jsonResponse));

    }

    [Fact]
    public async Task StartConversation_ReturnsInternalServerError_WhenExceptionOccurs()
    {
        // Arrange
        _ConversationServiceMock.Setup(s => s.StartConversation(It.IsAny<List<string>>(), It.IsAny<SendMessageDto>()))
            .ThrowsAsync(new Exception("Test exception")); 

        var httpContent = new StringContent(JsonConvert.SerializeObject(_startconversationRequest), Encoding.Default, "application/json");

        // Act
        var StartConversationResponse = await _httpClient.PostAsync($"/api/conversations/", httpContent);

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, StartConversationResponse.StatusCode);
    }

    [Fact]
    public async Task StartConversation_ReturnsConflict_OnConflictError()
    {
        // Arrange
        _ConversationServiceMock.Setup(s => s.StartConversation(It.IsAny<List<string>>(), It.IsAny<SendMessageDto>()))
            .ThrowsAsync(new HttpException(" a message already exist with this id ", 409));

        var httpContent = new StringContent(JsonConvert.SerializeObject(_startconversationRequest), Encoding.Default, "application/json");

        // Act
        var StartConversationResponse = await _httpClient.PostAsync($"/api/conversations/", httpContent);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, StartConversationResponse.StatusCode);
    }



    //[Fact]
    //public async Task StartConversation_ReturnsNotFound_OnNotFoundError()
    //{
    //    // Arrange
    //    _ConversationServiceMock.Setup(s => s.StartConversation(It.IsAny<List<string>>(), It.IsAny<SendMessageDto>()))
    //        .ThrowsAsync(new HttpException("The sender of the  message does not exist.", 404));


    //    var httpContent = new StringContent(JsonConvert.SerializeObject(_startconversationRequest), Encoding.Default, "application/json");

    //    // Act
    //    var StartConversationResponse = await _httpClient.PostAsync($"/api/conversations/", httpContent);

    //    // Assert
    //    Assert.Equal(HttpStatusCode.NotFound, StartConversationResponse.StatusCode);
    //}







        [Theory]
        [InlineData(null)]
        [InlineData("anytoken")]
        public async Task GetConversationOfUser_ReturnsOkResultWithResponse(string? continuationToken)
        {
            // Arrange
            var username = "testuser";
            var limit = 10;
            var lastSeenMessageTime = 123456789;

            string expectedNextUri = null;

            if (continuationToken != null)
            {
                expectedNextUri = $"/api/conversations?username=testuser&limit=10&lastSeenConversationTime=123456789&continuationToken={continuationToken}";
            }
            var expectedResponse = new GetConversationsOfUserResponse(It.IsAny<List<ConversationInfo>>(), expectedNextUri);

            _ConversationServiceMock.Setup(x => x.GetUserConversations(username, continuationToken, limit, lastSeenMessageTime))
                .ReturnsAsync(new GetConversationsOfUserServiceResponse(It.IsAny<List<ConversationInfo>>(), continuationToken));

            // Act
            var actualResponse = await _httpClient.GetAsync($"/api/conversations?username={username}&limit={limit}&lastSeenConversationTime={lastSeenMessageTime}&continuationToken={continuationToken}");

            var actualResonseJson = await actualResponse.Content.ReadAsStringAsync();
            var GetConversationOfUser = JsonConvert.DeserializeObject<GetConversationsOfUserResponse>(actualResonseJson);
            // Assert
            Assert.Equal(HttpStatusCode.OK, actualResponse.StatusCode);
            Assert.Equal(expectedNextUri, GetConversationOfUser.NextUri);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task GetConversationOfUser_InvalidRequest_ReturnsBadRequestResult(string username)
    {


        var continuationToken = "token";
        var limit = 10;
        var lastSeenMessageTime = 123456789;

        var result = await _httpClient.GetAsync($"/api/conversations?username={username}&limit={limit}&lastSeenConversationTime={lastSeenMessageTime}&continuationToken={continuationToken}");

        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);

    }


    [Fact]
    public async Task GetConversationOfUser_ExceptionThrown_ReturnsInternalServerErrorResult()
    {
        // Arrange
        var username = "testUser";
        var continuationToken = "token";
        var limit = 10;
        var lastSeenMessageTime = 123456789;
        _ConversationServiceMock.Setup(x => x.GetUserConversations(username, continuationToken, limit, lastSeenMessageTime))
            .ThrowsAsync(new Exception("Error"));

        var result = await _httpClient.GetAsync($"/api/conversations?username={username}&limit={limit}&lastSeenConversationTime={lastSeenMessageTime}&continuationToken={continuationToken}");

        Assert.Equal(HttpStatusCode.InternalServerError, result.StatusCode);

    }











    }



}









