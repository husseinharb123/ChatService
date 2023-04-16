using ChatService.Web.Configuration;
using Microsoft.Azure.Cosmos;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Options;
using ChatService.Web.Storage;
using ChatService.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();

builder.Services.Configure<CosmosSetting>(builder.Configuration.GetSection("cosmos"));
builder.Services.Configure<BlobStorageSettings>(builder.Configuration.GetSection("blobstorage"));


builder.Services.AddSingleton(
    sp => {
        var cosmosOption = sp.GetRequiredService<IOptions<CosmosSetting>>();
        return new CosmosClient(cosmosOption.Value.ConnectionString);

    });

builder.Services.AddSingleton(

    sp =>{
        var bolbOption = sp.GetRequiredService<IOptions<BlobStorageSettings>>();
        return new BlobServiceClient(bolbOption.Value.ConnectionString);
    });


builder.Services.AddSingleton<IProfileStore, CosmosProfileStore>();
builder.Services.AddSingleton<IConversationStore, CosmosConversationStore>();
builder.Services.AddSingleton<IMessageStore, CosmosMessageStore>();
builder.Services.AddSingleton<IImageStore, AzureBlobStorageImageStore>();



builder.Services.AddSingleton<IMessageService, MessageService>();
builder.Services.AddSingleton<IConversationService, ConversationService>();
builder.Services.AddSingleton<IProfileService, ProfileService>();




var app = builder.Build();
 
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();


public partial class Program { }