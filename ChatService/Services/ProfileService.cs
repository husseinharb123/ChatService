using ChatService.Web.Dtos;
using ChatService.Web.Storage;

namespace ChatService.Web.Services;

public class ProfileService : IProfileService
{

    private readonly IProfileStore _profileStore;

    public ProfileService(IProfileStore profileStore)
    {
        _profileStore = profileStore;
    }




    public Task CreateProfile(Profile profile)
    {
        return _profileStore.UpsertProfile(profile);
    }

    public Task<Profile?> GetProfile(string username)
    {
        return _profileStore.GetProfile(username);
    }

    public Task UpdateProfile(Profile profile)
    {
        return _profileStore.UpsertProfile(profile);
    }
}