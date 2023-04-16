using System.ComponentModel.DataAnnotations;

namespace ChatService.Web.Storage.Entities
{



    public record ProfileEntity (

        [Required]  string id ,
        [Required]  string partitionkey, 
        [Required]  string FirstName ,
        [Required]  string LastName,
        [Required]  string ProfilePictureid 
    );


}
