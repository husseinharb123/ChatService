namespace ChatService.Web.Dtos
{
    public record ConversationInfo(string Id, long lastModifiedUnixTime, Profile recipient);
}
