using ChatService.Web.Dtos;
using ChatService.Web.Exceptions;
using ChatService.Web.Storage;
using Microsoft.Azure.Cosmos;

namespace ChatService.Web.Services
{
    public class MessageService : IMessageService
    {

        private readonly IConversationStore _conversationStore;
        private readonly IMessageStore _messageStore;
        private readonly IProfileStore _profileStore;

        public MessageService(IConversationStore conversationStore, IMessageStore messageStore, IProfileStore profileStore)
        {
            _conversationStore = conversationStore;
            _messageStore = messageStore;
            _profileStore = profileStore;
        }


        public async Task<GetConversationMessageServiceResponse> GetConversationMessages(string conversationId, string? continuationToken, int limit, long lastSeenMessageTime)
        {

            try 
            {
                var messages = new List<GetMessageDto>();

                var getMessagesResponse = await _messageStore.GetConersationMessages(conversationId, continuationToken, limit, lastSeenMessageTime);
                var conversationMessages = getMessagesResponse.Messages;
                var lastContinuationToken = getMessagesResponse.ContinuationToken;

                var tasks = conversationMessages.Select(async message =>
                {
                    return new GetMessageDto(
                        Text: message.Text,
                        SenderUsername: message.SenderId,
                        UnixTime: message.CreatedUnixTime
                    );
                });

                messages.AddRange(await Task.WhenAll(tasks));

                return new GetConversationMessageServiceResponse(messages, lastContinuationToken);

            }
            catch 
            {
                throw;
            }
        }



        public async Task<long> SendMessage(string conversationId, SendMessageDto message)
        {

            var userConversation = await _conversationStore.GetUserConversation(conversationId, message.SenderUsername) 
                ?? throw new HttpException("no conversation with this username", 404);

            try
            {

                //var senderUsername = message.SenderUsername;
                //var senderProfile = await _profileStore.GetProfile(senderUsername);

                //if (senderProfile == null)
                //{

                //   throw new HttpException("The sender of the  message does not exist.", 404);
                //}


                var sendMessageResponse = await _messageStore.CreateMessage(message,conversationId);
                var updateUserConversationResponse = await _conversationStore.UpsertUserConversation(userConversation);
                var CreatedUnixTime = updateUserConversationResponse;
                return sendMessageResponse;
            }

            catch
            {

                throw;

            }
        }




    }
}
