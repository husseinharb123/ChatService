using ChatService.Web.Dtos;
using ChatService.Web.Exceptions;
using ChatService.Web.Services;
using Microsoft.AspNetCore.Mvc;
namespace ChatService.Web.Controllers
{
    [ApiController]
    public class MessageController : ControllerBase
    {
        private readonly IMessageService _messageService;
        private readonly IConversationService _conversationService;
        private readonly IProfileService _profileService;

        public MessageController( IMessageService messageService,IConversationService conversationService, IProfileService profileService)
        {
            _messageService = messageService;
            _conversationService = conversationService;
            _profileService = profileService;
        }




        [HttpPost]
        [Route("api/conversations/{conversationId}/messages")]
        public async Task<ActionResult<SendMessageResponse>> SendMessageToConversation(string conversationId, [FromBody] SendMessageDto message)
        {
            if (message == null)
            {
                return BadRequest("The request must have a body.");
            }

            try
            {

                var response = await _messageService.SendMessage(conversationId, message);

                return CreatedAtAction(nameof(SendMessageToConversation), new SendMessageResponse(response));
            }
            catch (HttpException ex)
            {
                if (ex.StatusCode == 409)
                {
                    return Conflict(ex.Message);
                }
                //else if (ex.StatusCode == 404)
                //{
                //    return NotFound(ex.Message);
                //}
                else {
                    throw; 
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }

        }

     
        
        
        
        
        
        
        
        
        [HttpGet]
        [Route("api/conversations/{conversationId}/messages")]
        public async Task<ActionResult<GetConversationMessageResponse>> GetMessagesOfConversation(string conversationId,string? continuationToken,int limit, long lastSeenMessageTime)
        {
            try
            {

               Console.WriteLine(lastSeenMessageTime.ToString());

                string nextUri = null;

                var response = await _messageService.GetConversationMessages(conversationId,continuationToken,limit, lastSeenMessageTime);

                var lastContinuationToken = response.ContinuationToken;

                if (!string.IsNullOrEmpty(lastContinuationToken))
                {
                    nextUri = $"/api/conversations/{conversationId}/messages?&limit={limit}&lastSeenMessageTime={lastSeenMessageTime}&continuationToken={lastContinuationToken}";
                }

                return Ok(new GetConversationMessageResponse(response.Messages, nextUri));

            }
            catch (HttpException ex)
            {

                ////if (ex.StatusCode == 404)
                ////{
                ////    return NotFound(ex.Message);
                ////}

                    throw; 

            }
            catch (Exception ex) 
            {
                return StatusCode(500, ex.Message);
            }

        }
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    }

}