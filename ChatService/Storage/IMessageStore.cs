using ChatService.Web.Dtos;




namespace ChatService.Web.Storage
{
    public interface IMessageStore
    {
        Task<long> CreateMessage( SendMessageDto sendMessage,string conversationId);

        Task<GetConverationMessagesDto> GetConersationMessages( string conversationId,string? continuationToken, int limit, long lastSeenMessageTime);


    
    }
}
