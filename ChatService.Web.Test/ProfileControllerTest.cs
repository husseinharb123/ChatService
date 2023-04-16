using System.Net;
using System.Text;
using ChatService.Web.Dtos;
using ChatService.Web.Storage;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Newtonsoft.Json;

namespace ChatService.Web.Test;
public class ProfileControllerTest : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly Mock<IProfileStore> _profileStoreMock = new();
    private readonly HttpClient _httpClient;
    private readonly Profile _profile; 

    public ProfileControllerTest(WebApplicationFactory<Program> factory)
    {
        _httpClient = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services => { services.AddSingleton(_profileStoreMock.Object); });
        }).CreateClient();

        _profile = new Profile("foobar", "Foo", "Bar", "imgid");
    }

    [Fact]
    public async Task GetProfile_ShoudReturnProfileWithOkStatus()
    {
        //setup 
        
        _profileStoreMock.Setup(m => m.GetProfile(_profile.Username))
            .ReturnsAsync(_profile);

        //ack 
        var responseProfile = await _httpClient.GetAsync($"api/Profile/{_profile.Username}");
        
        // assert 
        Assert.Equal(HttpStatusCode.OK, responseProfile.StatusCode);

        var json = await responseProfile.Content.ReadAsStringAsync();

        Assert.Equal(_profile, JsonConvert.DeserializeObject<Profile>(json));
    }

    [Fact]
    public async Task GetProfile_ShoudReturnNullWithNotFoundStatus()
    {
        //setup
        _profileStoreMock.Setup(m => m.GetProfile("foobar"))
            .ReturnsAsync((Profile?)null);
        //ack 
        var responseProfile = await _httpClient.GetAsync($"api/Profile/foobar");
        
        //assert
        Assert.Equal(HttpStatusCode.NotFound, responseProfile.StatusCode);
    }

    [Fact]
    public async Task AddProfile_ShouldAddProfileWithCreatedStatus()
    {
        //setup
        var httpContent = new StringContent(JsonConvert.SerializeObject(_profile), Encoding.Default, "application/json");
        
        //ack 
        var response = await _httpClient.PostAsync("api/Profile", httpContent);

        //assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal("http://localhost/api/Profile/foobar", response.Headers.GetValues("Location").First());

        _profileStoreMock.Verify(mock => mock.UpsertProfile(_profile), Times.Once);
    }

    [Fact]
    public async Task AddProfile_ShouldReturnConflictStatus()
    {
        //setup 
        _profileStoreMock.Setup(m => m.GetProfile(_profile.Username))
            .ReturnsAsync(_profile);
        
        //ack
        var httpContent = new StringContent(JsonConvert.SerializeObject(_profile), Encoding.Default, "application/json");
        var response = await _httpClient.PostAsync("api/Profile", httpContent);
        
        //assert 
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

        _profileStoreMock.Verify(m => m.UpsertProfile(_profile), Times.Never);
    }

    [Theory]
    [InlineData(null, "Foo", "Bar", "imgid")]
    [InlineData("", "Foo", "Bar", "imgid")]
    [InlineData(" ", "Foo", "Bar", "imgid")]
    [InlineData("foobar", null, "Bar", "imgid")]
    [InlineData("foobar", "", "Bar", "imgid")]
    [InlineData("foobar", "   ", "Bar", "imgid")]
    [InlineData("foobar", "Foo", "", "imgid")]
    [InlineData("foobar", "Foo", null, "imgid")]
    [InlineData("foobar", "Foo", " ", "imgid")]

    public async Task AddProfile_InvalidArgs_ShouldReturn(string username, string firstName, string lastName,string profileimageid)
    {
       //setup 
        var profile = new Profile(username, firstName, lastName, profileimageid);
       
       //ack
       
        var response = await _httpClient.PostAsync("api/Profile",
            new StringContent(JsonConvert.SerializeObject(profile), Encoding.Default, "application/json"));
        //assert

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        _profileStoreMock.Verify(mock => mock.UpsertProfile(profile), Times.Never);
    }

   





    }
