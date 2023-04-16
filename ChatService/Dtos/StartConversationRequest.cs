namespace ChatService.Web.Dtos
{


    public record StartConversationRequest(List<string> Participants, SendMessageDto FirstMessage);


}
