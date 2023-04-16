using System.ComponentModel.DataAnnotations;

namespace ChatService.Web.Dtos
{
    public record UploadImageResponse(

        [Required]  string ImageId 
    );
}
