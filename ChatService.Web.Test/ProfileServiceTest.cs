using ChatService.Web.Dtos;
using ChatService.Web.Exceptions;
using ChatService.Web.Services;
using ChatService.Web.Storage;
using Microsoft.VisualBasic;
using Moq;


namespace ChatService.Web.Test;







public class ProfileServiceTest
{

    private readonly Mock<IProfileStore> _ProfileStoreMock = new();

    private readonly ProfileService _profileService;
    private readonly Profile _profile;
    public ProfileServiceTest()
    {
        _profileService = new ProfileService(_ProfileStoreMock.Object);
        _profile = new Profile("foobar", "Foo", "Bar", "imgid");
    }



    [Fact]
    public async Task CreateProfile_ShouldCallUpsertProfileWithCorrectProfile()
    {
        // Arrange
        _ProfileStoreMock.Setup(x => x.UpsertProfile(It.IsAny<Profile>()))
            .Returns(Task.CompletedTask);

        // Act
        await _profileService.CreateProfile(_profile);

        // Assert
        _ProfileStoreMock.Verify(x => x.UpsertProfile(It.Is<Profile>(p =>
            p.Username == _profile.Username &&
            p.FirstName == _profile.FirstName &&
            p.LastName == _profile.LastName &&
            p.ProfilePictureid == _profile.ProfilePictureid)), Times.Once);
    }

    [Fact]
    public async Task GetProfile_ShouldCallGetProfileWithCorrectUsername()
    {
        // Arrange
        _ProfileStoreMock.Setup(x => x.GetProfile(It.IsAny<string>()))
            .Returns(Task.FromResult(_profile));

        // Act
        await _profileService.GetProfile(_profile.Username);

        // Assert
        _ProfileStoreMock.Verify(x => x.GetProfile(_profile.Username), Times.Once);
    }

    [Fact]
    public async Task UpdateProfile_ShouldCallUpsertProfileWithCorrectProfile()
    {
        // Arrange
        _ProfileStoreMock.Setup(x => x.UpsertProfile(It.IsAny<Profile>()))
            .Returns(Task.CompletedTask);

        // Act
        await _profileService.UpdateProfile(_profile);

        // Assert
        _ProfileStoreMock.Verify(x => x.UpsertProfile(It.Is<Profile>(p =>
            p.Username == _profile.Username &&
            p.FirstName == _profile.FirstName &&
            p.LastName == _profile.LastName &&
            p.ProfilePictureid == _profile.ProfilePictureid)), Times.Once);
    }






}