using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Options;
using ChatService.Web.Configuration;

namespace ChatService.Web.Storage
{
    public class AzureBlobStorageImageStore : IImageStore
    {
        private readonly BlobContainerClient _blobContainerClient;

        public AzureBlobStorageImageStore(IOptions<BlobStorageSettings> options)
        {
            _blobContainerClient = new BlobContainerClient(options.Value.ConnectionString,
                options.Value.ProfilePicturesContainerName);
        }

        public async Task<string> Upload(byte[] imageData)
        {
            string id = Guid.NewGuid().ToString();
            using var stream = new MemoryStream(imageData);
            await _blobContainerClient.UploadBlobAsync(id, stream);

            // List all blobs in the container
            await foreach (BlobItem blobItem in _blobContainerClient.GetBlobsAsync())
            {
                Console.WriteLine("\t" + blobItem.Name);
            }

            return id;
        }

        public async Task<byte[]> Download(string blobName)
        {
            var response = await _blobContainerClient.GetBlobClient(blobName)
                .DownloadAsync();
            await using var memoryStream = new MemoryStream();
            await response.Value.Content.CopyToAsync(memoryStream);
            var bytes = memoryStream.ToArray();
            return bytes;
        }

        public async Task Delete(string id)
        {
            await _blobContainerClient.DeleteBlobIfExistsAsync(id);
        }
    }
}