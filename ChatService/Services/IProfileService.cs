using ChatService.Web.Dtos;

namespace ChatService.Web.Services;

public interface IProfileService
{

    Task CreateProfile(Profile profile);
    Task<Profile?> GetProfile(string username);
    Task UpdateProfile(Profile profile);
}