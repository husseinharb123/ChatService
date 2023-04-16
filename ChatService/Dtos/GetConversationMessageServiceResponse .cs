namespace ChatService.Web.Dtos
{
    public record GetConversationMessageServiceResponse(List<GetMessageDto>Messages,string ? ContinuationToken);

}
