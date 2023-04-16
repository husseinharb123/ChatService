using ChatService.Web.Dtos;


namespace ChatService.Web.Storage
{
    public interface IConversationStore
    {
        Task<long> UpsertUserConversation( UserConversation userConversation );
        Task<UserConversation?> GetUserConversation(string conversationId, string username);
        Task<long> CreateUserConversation(UserConversation userConversation);
        Task<GetUserConversationDto> GetUserConversations(string username, string? continuationToken, int  limit, long  lastSeenMessageTime);


    }

}
