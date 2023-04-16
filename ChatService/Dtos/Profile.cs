using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace ChatService.Web.Dtos
{
    public record Profile(
        [Required] string Username,
        [Required] string FirstName,
        [Required] string LastName,
        string? ProfilePictureid 
    );

}
