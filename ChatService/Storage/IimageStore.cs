namespace ChatService.Web.Storage
{
    public interface IImageStore
    {
        Task Delete(string id);
        Task<byte[]> Download(string id);
        Task<string> Upload(byte[] imageData);
    }
}