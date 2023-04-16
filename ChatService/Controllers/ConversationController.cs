using ChatService.Web.Dtos;
using ChatService.Web.Exceptions;
using ChatService.Web.Services;

using Microsoft.AspNetCore.Mvc;

namespace ChatService.Web.Controllers;


[ApiController]

public class ConversationController : ControllerBase
{
    private readonly IConversationService _conversationsService;
    private readonly IMessageService _messageService;
    private readonly IProfileService _profileService;

    public ConversationController(IConversationService conversationsService, IMessageService messageService, IProfileService profileService)
    {
        _conversationsService = conversationsService;
        _messageService = messageService;
        _profileService = profileService;
    }

    [HttpPost]
    [Route("api/conversations")]
        public async Task<ActionResult<StartConversationResponse>> StartConversation([FromBody] StartConversationRequest request)
        {
            if (request == null)
            {
                return BadRequest("The request must have a body.");
            }

            if (request.Participants == null || request.FirstMessage == null)
            {
                return BadRequest("The request must include participants and a first message..");
            }

            try
            {

                var response = await _conversationsService.StartConversation(request.Participants, request.FirstMessage);
                return CreatedAtAction(nameof(StartConversation), response);
            }

            catch (HttpException ex)
            {
                if (ex.StatusCode == 409)
                {
                    return Conflict(ex.Message);
                }
                //if (ex.StatusCode == 404)
                //{
                //    return NotFound(ex.Message);
                //}
   
                    throw;
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }


    [HttpGet]
    [Route("api/conversations")]
    public async Task<ActionResult<GetConversationsOfUserResponse>> GetConversationOfUser(string username, string? continuationToken, int limit, long lastSeenConversationTime)
    {
        if (string.IsNullOrEmpty(username))
        {
            return BadRequest("username cannot be null or empty");
        }

        try
        {

            string nextUri = null;
            var response = await _conversationsService.GetUserConversations(username, continuationToken, limit, lastSeenConversationTime);

            var lastContinuationToken = response.ContinuationToken;

            if (!string.IsNullOrEmpty(lastContinuationToken))
            {
                nextUri = $"/api/conversations?username={username}&limit={limit}&lastSeenConversationTime={lastSeenConversationTime}&continuationToken={lastContinuationToken}";
            }

            return Ok(new GetConversationsOfUserResponse(response.ConversationsInfo, nextUri));
        }


       
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
}