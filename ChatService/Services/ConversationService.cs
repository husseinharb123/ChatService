using ChatService.Web.Dtos;
using ChatService.Web.Exceptions;
using ChatService.Web.Storage;
using Org.BouncyCastle.Cms;

namespace ChatService.Web.Services
{
    public class ConversationService : IConversationService
    {


        private readonly IConversationStore _conversationStore;
        private readonly IMessageStore _messageStore;
        private readonly IProfileStore _profileStore;

        public ConversationService(IConversationStore conversationStore, IMessageStore messageStore, IProfileStore profileStore)
        {
            _conversationStore = conversationStore;
            _messageStore = messageStore;
            _profileStore = profileStore;
        }



        public async Task<StartConversationResponse> StartConversation(List<string> Participants, SendMessageDto FirstMessage)
        {



            var conversationId = Participants[0] + "_" + Participants[1];

            var userConveration = new UserConversation
                (
                   Username : Participants[0],
                   ConversationId : conversationId,
                   Participant : Participants[1],
                   LastModifiedUnixTime : 0 

                );

            try
            {
                //var firstMessageSenderUsername = FirstMessage.SenderUsername;
                //var firstSenderExists = await _profileStore.GetProfile(firstMessageSenderUsername);

                //if (firstSenderExists == null)
                //{
                //   throw new HttpException( "The sender of the first message does not exist.",404);
                //}

                //var participantsExist = true;
                //foreach (var username in Participants)
                //{
                //    var profile = await _profileStore.GetProfile(username);
                //    if (profile == null)
                //    {
                //        participantsExist = false;
                //        break;
                //    }
                //}

                //if (!participantsExist)
                //{
                //    throw new HttpException("One or more participants do not exist.", 404);
                //}
                var sendMessageResponse = await _messageStore.CreateMessage(FirstMessage,conversationId);

                var createUserConversationResponse = await _conversationStore.CreateUserConversation(userConveration);

                var CreatedUnixTime = createUserConversationResponse;

                return new StartConversationResponse(conversationId, CreatedUnixTime);
            }

            catch
            {

                throw;

            }

        }


        public async Task<GetConversationsOfUserServiceResponse> GetUserConversations(string username, string? continuationToken, int limit, long lastSeenMessageTime)
        {
            try
            {
                var conversationsInfo = new List<ConversationInfo>();

                var getUserConversationsResponse = await _conversationStore.GetUserConversations(username, continuationToken, limit, lastSeenMessageTime);

                var userConversations = getUserConversationsResponse.UserConversations;
                var LastcontinuationToken = getUserConversationsResponse.ContinuationToken;

                var tasks = userConversations.Select(async conversation =>
                {
                    var recipient = await _profileStore.GetProfile(conversation.Participant);
                   
                    return new ConversationInfo(
                        Id: conversation.ConversationId,
                        lastModifiedUnixTime: conversation.LastModifiedUnixTime,
                        recipient: recipient
                    );
                });

                conversationsInfo.AddRange(await Task.WhenAll(tasks));

                return new GetConversationsOfUserServiceResponse(conversationsInfo, LastcontinuationToken);
            }
            catch
            {
                throw;
            }
        }













        public async  Task<long> UpdateUserConversation(UserConversation userConversation)
        {



            try
            {

                var UpsertUserConversationResponse = await _conversationStore.UpsertUserConversation(userConversation);
                var CreatedUnixTime = UpsertUserConversationResponse;

                return CreatedUnixTime;
            }

            catch
            {

                throw;

            }
        }
    }
}
