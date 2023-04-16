
using ChatService.Web.Dtos;
using ChatService.Web.Exceptions;
using ChatService.Web.Storage.Entities;
using Microsoft.Azure.Cosmos;

using System.Net;
using System.Security.Policy;
using System.Web;

namespace ChatService.Web.Storage
{




    public class CosmosConversationStore : IConversationStore
    {


        private readonly CosmosClient _cosmosclient;


        private Container ConversationStoreContainer => _cosmosclient.GetDatabase("ChatService").GetContainer("userconversation");
        public CosmosConversationStore(CosmosClient cosmosclient)
        {

            _cosmosclient = cosmosclient;

        }



        public static UserConversationEntiy ToEntity(UserConversation userConversation)
        {

            return new UserConversationEntiy(
                id: userConversation.ConversationId,
                username : userConversation.Username,
                partitionkey: userConversation.Username,
                ConversationId: userConversation.ConversationId,
                Participant: userConversation.Participant,
                LastModifiedUnixTime: userConversation.LastModifiedUnixTime

                );

        }

        public static UserConversation ToUserConversation(UserConversationEntiy userConversationEntiy)
        {

            return new UserConversation(
                Username: userConversationEntiy.username,
                Participant: userConversationEntiy.Participant,
                ConversationId: userConversationEntiy.ConversationId,
                LastModifiedUnixTime: userConversationEntiy.LastModifiedUnixTime
                );

        }




        public async Task<long> UpsertUserConversation(UserConversation userConversation)
        {
            try
            {
                var currentUnixTime = UnixTimeNow();
                var username1 = userConversation.Username;
                var username2 = userConversation.Participant;

                var user1Conversation = new UserConversation
                (
                    Username : username1,
                    ConversationId: userConversation.ConversationId,
                    Participant: username2,
                    LastModifiedUnixTime: currentUnixTime
                );

                var user2Conversation = new UserConversation
                (
                    Username : username2,
                    ConversationId: userConversation.ConversationId,
                    Participant: username1,
                    LastModifiedUnixTime: currentUnixTime
                );

                await Task.WhenAll(
                    ConversationStoreContainer.UpsertItemAsync(ToEntity(user1Conversation)),
                    ConversationStoreContainer.UpsertItemAsync(ToEntity(user2Conversation))
                );
                return currentUnixTime;
            }
            catch (CosmosException ex)
            {
                if (ex.StatusCode == HttpStatusCode.NotFound)
                {
                    throw new HttpException("no converation with this id exist ", 404);
                }
                throw;
            }
            catch
            {
                throw;
            }
        }

        public async Task<long> CreateUserConversation(UserConversation userConversation)
        {
            try
            {
                var currentUnixTime = UnixTimeNow();
                var username1 = userConversation.Username;
                var username2 = userConversation.Participant;

                var user1Conversation = new UserConversation
                (
                    Username: username1,
                    ConversationId: userConversation.ConversationId,
                    Participant: username2,
                    LastModifiedUnixTime: currentUnixTime
                );

                var user2Conversation = new UserConversation
                (
                    Username: username2,
                    ConversationId: userConversation.ConversationId,
                    Participant: username1,
                    LastModifiedUnixTime: currentUnixTime
                );

                await Task.WhenAll(
                    ConversationStoreContainer.CreateItemAsync(ToEntity(user1Conversation)),
                    ConversationStoreContainer.CreateItemAsync(ToEntity(user2Conversation))
                );
                return currentUnixTime;
            }
            catch (CosmosException ex) {
                if (ex.StatusCode == HttpStatusCode.Conflict) {
                    throw new HttpException("converation with this Already exist ", 409);
                }
                throw;
            }
            catch
            {
                throw;
            }
        }


        public async Task<UserConversation?> GetUserConversation(string conversationId, string username)
        {

            try
            {
                var sqlQuery = new QueryDefinition(
                    $"SELECT c.id ,c.username ,c.ConversationId , c.partitionkey ,c.Participant ,c.LastModifiedUnixTime " +
                    $"FROM c WHERE c.ConversationId = @conversationId")
                    .WithParameter("@conversationId", conversationId);

                var queryIterator = ConversationStoreContainer.GetItemQueryIterator<UserConversationEntiy>(
                    sqlQuery,
                    requestOptions: new QueryRequestOptions
                    {
                        ConsistencyLevel = ConsistencyLevel.Session,
                        PartitionKey = new PartitionKey(username)

                    }); ;

                var entity = await queryIterator.ReadNextAsync();
                var userConversationEntity = entity.FirstOrDefault();
                return userConversationEntity != null ? ToUserConversation(userConversationEntity) : null;
            }

            catch
            {
                throw;
            }
        }




        public async Task<GetUserConversationDto> GetUserConversations(string username, string? continuationToken, int limit, long lastSeenMessageTime)
        {

            var queryText =
                "SELECT " +
                "c.id ," +
                "c.username ," +
                "c.ConversationId ," +
                "c.partitionkey ," +
                "c.Participant ," +
                "c.LastModifiedUnixTime " +
                "FROM c " +
                "WHERE c.username = @username AND " +
                "c.LastModifiedUnixTime > @lastSeenMessageTime " +
                "ORDER BY c.LastModifiedUnixTime DESC";

            var queryDefinition = new QueryDefinition(queryText)
                .WithParameter("@username", username)
                .WithParameter("@lastSeenMessageTime", lastSeenMessageTime);

            var userConversations = new List<UserConversation>();

            try
            {
                using (var feedIterator = ConversationStoreContainer.GetItemQueryIterator<UserConversationEntiy>(
                    queryDefinition,
                    continuationToken: continuationToken,
                    requestOptions: new QueryRequestOptions { MaxItemCount = limit, PartitionKey = new PartitionKey(username) }))
                {
                    var queryResponse = await feedIterator.ReadNextAsync().ConfigureAwait(false);
                    foreach (var userConversationEntity in queryResponse.Resource)
                    {
                        var userConversation = ToUserConversation(userConversationEntity);
                        userConversations.Add(userConversation);
                    }
                    continuationToken = queryResponse.ContinuationToken;
                }

                if (continuationToken != null)
                {
                    continuationToken = WebUtility.UrlEncode(continuationToken);
                }

                return new GetUserConversationDto(userConversations, continuationToken);
            }
            catch (CosmosException ex)
            {

                throw new Exception("Failed to retrieve user conversations.", ex);
            }

            catch {
                throw; 
            }


        }









        public long UnixTimeNow()
        {
            var timeSpan = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0));
            return (long)timeSpan.TotalSeconds;
        }

    }



  

}