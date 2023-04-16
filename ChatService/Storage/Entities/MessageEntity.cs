namespace ChatService.Web.Dtos
{
    public record MessageEntity(string id ,string partitionkey, string Text ,long CreatedUnixTime,string ConversationId,string SenderId);
}
