using ChatService.Web.Dtos;
using ChatService.Web.Storage;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;




namespace ChatService.Web.IntegrationTest
{
    public class CosmosProfileStoreTest : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly IProfileStore _store;


        private readonly Profile _profile = new(
            Username: Guid.NewGuid().ToString(),
            FirstName: "Foo",
            LastName: "Bar",
            ProfilePictureid: "djgksfksf"
        );






        public CosmosProfileStoreTest(WebApplicationFactory<Program> factory)
        {
            _store = factory.Services.GetRequiredService<IProfileStore>();
        }





        [Fact]
        public async Task AddNewProfile_WithValidProfile_ShouldAddProfile()
        {
            //setup
            await _store.UpsertProfile(_profile);

            //act
            var responseProfile = await _store.GetProfile(_profile.Username);

            //assert
            Assert.NotNull(responseProfile);
            Assert.Equal(_profile, responseProfile);



        }



        [Theory]
        [InlineData(null, "Foo", "Bar", "imgid")]
        [InlineData("", "Foo", "Bar", "imgid")]
        [InlineData(" ", "Foo", "Bar", "imgid")]
        [InlineData("foobar", null, "Bar", "imgid")]
        [InlineData("foobar", "", "Bar", "imgid")]
        [InlineData("foobar", "   ", "Bar", "imgid")]
        [InlineData("foobar", "Foo", "", "imgid")]
        [InlineData("foobar", "Foo", null, "imgid")]
        [InlineData("foobar", "Foo", " ", "imgid")]
        //[InlineData("foobar", "Foo", "Bar ", null)]
        //[InlineData("foobar", "Foo", "Bar ", "")]

        public async Task AddNewProfile_withInvalidArg_ShouldThrowArgumentException(string username, string firstname, string lastname, string profileimageid)
        {

            //setup
            var profile = new Profile(username, firstname, lastname, profileimageid);

            // ack + assert

            await Assert.ThrowsAsync<ArgumentException>(

                async () => await _store.UpsertProfile(profile)

                );

        }

        [Fact]
        public async Task AddNewProfile_withNullProfile_ShouldThrowArgumentException()
        {
            // setup
            Profile? profile = null;

            // + act + assert
            await Assert.ThrowsAsync<ArgumentException>(

                async () => await _store.UpsertProfile(profile)

                );

        }



        [Fact]

        public async Task GetProfile_WithGivenExistingProfile_ShoudReturnProfile()
        {
            //setup
            await _store.UpsertProfile(_profile);

            // ack 
            var responseProfile = await _store.GetProfile(_profile.Username);

            //assert 
            Assert.NotNull(responseProfile);
            Assert.Equal(_profile, responseProfile);

        }


        [Fact]
        public async Task GetProfile_WithNonExistingProfile_ShoudReturnNull()
        {
            //ack 
            var responseProfile = await _store.GetProfile(_profile.Username);

            //  assert
            Assert.Null(responseProfile);
        }

     



    }

}