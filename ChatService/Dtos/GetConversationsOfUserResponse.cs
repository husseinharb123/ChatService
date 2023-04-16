namespace ChatService.Web.Dtos
{
    public record GetConversationsOfUserResponse(List<ConversationInfo> Conversations, string? NextUri);


}
