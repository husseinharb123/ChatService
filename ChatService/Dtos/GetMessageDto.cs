namespace ChatService.Web.Dtos
{
    public record GetMessageDto(string Text ,string SenderUsername, long UnixTime);
}
