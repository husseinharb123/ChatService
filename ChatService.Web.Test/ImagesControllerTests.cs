using System.Net.Http.Headers;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Newtonsoft.Json;
using ChatService.Web.Dtos;
using ChatService.Web.Storage;

namespace ChatService.Web.Test;

public class ImagesControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly Mock<IImageStore> _imageStoreMock = new();
    private readonly HttpClient _httpClient;

    public ImagesControllerTests(WebApplicationFactory<Program> factory)
    {
        // DRY: Don't repeat yourself
        _httpClient = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services => { services.AddSingleton(_imageStoreMock.Object); });
        }).CreateClient();
    }

    [Fact]
    public async Task UploadImages()
    {
        string str = Guid.NewGuid().ToString();
        var bytes = Encoding.UTF8.GetBytes(str);
        var stream = new MemoryStream(bytes);

        HttpContent fileStreamContent = new StreamContent(stream);
        fileStreamContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
        {
            Name = "file",
            FileName = "anything"
        };

        using var formData = new MultipartFormDataContent();
        formData.Add(fileStreamContent);

        await _httpClient.PostAsync("api/images", formData);
        
        _imageStoreMock.Verify(m => m.Upload(bytes));
    }
}