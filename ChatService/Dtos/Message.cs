namespace ChatService.Web.Dtos
{
    public record Message(string Id , string Text ,long CreatedUnixTime,string ConversationId,string SenderId);
}
