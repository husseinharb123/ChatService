using ChatService.Web.Dtos;
using ChatService.Web.Storage;
using Microsoft.AspNetCore.Mvc;


namespace ChatService.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProfileController : ControllerBase

    {


        private readonly IProfileStore _profileStore;


        public ProfileController(IProfileStore profilestore)
        {
            _profileStore = profilestore;
        }

        [HttpPost]
        
        public async Task<ActionResult<Profile>> AddProfile(Profile profile)
        {
            if (profile == null ||
                string.IsNullOrWhiteSpace(profile.Username) ||
                string.IsNullOrWhiteSpace(profile.FirstName) ||
                string.IsNullOrWhiteSpace(profile.LastName) 
                
                )
            {
                return BadRequest($" Invalid {profile}");
            }

            try   
            {
            
                var responseExistingProfile = await _profileStore.GetProfile(profile.Username);
            
                if (responseExistingProfile != null)
                {
                    return Conflict($"A user with username {profile.Username} already exists");
                }

                await _profileStore.UpsertProfile(profile);
                return CreatedAtAction(nameof(GetProfile), new { username = profile.Username },profile);
            
            }
            catch 
            
            {
                return StatusCode(500, "An internal server error occurred.");
            }

        }

        [HttpGet("{username}")]
        public async Task<ActionResult<Profile>> GetProfile(string username)


        {

            if (string.IsNullOrWhiteSpace(username))
            {
                return BadRequest($"{nameof(username)} cannot be null or Whitespace");
            }

            try
            {
                var responseprofile = await _profileStore.GetProfile(username);

                if (responseprofile == null)
                {
                    return NotFound($"A User with username {username} was not found");
                }

                return Ok(responseprofile);

            }
            catch
            {
                return StatusCode(500, "An internal server error occurred.");

            }
        }

     



    }











}

