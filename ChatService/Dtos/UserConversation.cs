
namespace ChatService.Web.Dtos
{
    public record UserConversation(string Username ,string ConversationId, string Participant ,long LastModifiedUnixTime);
}
