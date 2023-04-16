namespace ChatService.Web.Dtos
{
    public record GetConversationsOfUserServiceResponse(List<ConversationInfo> ConversationsInfo, string? ContinuationToken);


}

