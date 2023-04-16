using ChatService.Web.Dtos;
using ChatService.Web.Storage;

namespace ChatService.Web.Services
{
    public interface IConversationService
    {

        

        Task<StartConversationResponse> StartConversation(List<string> Participants, SendMessageDto FirstMessage);
        Task<GetConversationsOfUserServiceResponse> GetUserConversations(string username, string? continuationToken, int limit, long lastSeenMessageTime);

        Task<long> UpdateUserConversation(UserConversation userConversation);

    }
}
