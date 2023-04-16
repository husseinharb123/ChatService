using System.ComponentModel.DataAnnotations;

namespace ChatService.Web.Dtos
{
    public record PutProfileRequest(
    
        [Required]  string FirstName ,
        [Required]  string LastName ,
        string? ProfilePictureid

    );

    
}
