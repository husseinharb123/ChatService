using System.ComponentModel.DataAnnotations;

namespace ChatService.Web.Storage.Entities
{
    public record UserConversationEntiy(
        [Required]  string id,
        [Required] string username,
        [Required] string ConversationId,
        [Required]  string partitionkey,
        [Required]  string Participant,
        [Required]  long LastModifiedUnixTime
        
        );
}