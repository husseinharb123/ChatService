using ChatService.Web.Dtos;

namespace ChatService.Web.Services
{
    public interface  IMessageService

    {

       
        Task<long> SendMessage(string conversationId, SendMessageDto message);

        Task<GetConversationMessageServiceResponse> GetConversationMessages(string conversationId, string? continuationToken, int limit, long lastSeenMessageTime);


    }
}
