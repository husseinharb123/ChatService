
namespace ChatService.Web.Dtos
{
    public record GetUserConversationDto(List<UserConversation> UserConversations, string? ContinuationToken);
}
