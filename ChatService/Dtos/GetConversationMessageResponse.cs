namespace ChatService.Web.Dtos
{
    public record GetConversationMessageResponse(List<GetMessageDto>Messages, string? NextUri);

}
