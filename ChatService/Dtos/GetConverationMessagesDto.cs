namespace ChatService.Web.Dtos
{
    public record GetConverationMessagesDto(List<Message> Messages,string? ContinuationToken);
}
