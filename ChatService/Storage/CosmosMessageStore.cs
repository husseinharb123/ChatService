
using ChatService.Web.Dtos;
using ChatService.Web.Exceptions;
using Microsoft.Azure.Cosmos;
using System.Net;



namespace ChatService.Web.Storage
{




    public class CosmosMessageStore : IMessageStore
    {


        private readonly CosmosClient _cosmosclient;

        private Container MessageStoreContainer => _cosmosclient.GetDatabase("ChatService").GetContainer("message");
        public CosmosMessageStore(CosmosClient cosmosclient)
        {
            _cosmosclient = cosmosclient;
        }

        public async Task<long> CreateMessage(SendMessageDto sendMessageRequest, string conversationId)
        {
            var CurrentUnixTime = UnixTimeNow();

            try
            {
                var message = new Message(
                     Id: sendMessageRequest.Id,
                     Text: sendMessageRequest.Text,
                     CreatedUnixTime: CurrentUnixTime,
                     ConversationId: conversationId,
                     SenderId: sendMessageRequest.SenderUsername
                    );

                await MessageStoreContainer.CreateItemAsync(ToEntity(message));


                return CurrentUnixTime;
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.Conflict)
            {

                throw new HttpException(" a message already exist with this id ", 409);
            }
            catch
            {

                throw;

            }
        }



        public async Task<GetConverationMessagesDto> GetConersationMessages(string conversationId, string? continuationToken, int limit, long lastSeenMessageTime)
        {

            var queryText =
                "SELECT " +
                "c.id ," +
                "c.partitionkey ," +
                "c.Text ," +
                "c.CreatedUnixTime , " +
                "c.ConversationId ,c.SenderId " +
                "FROM c " +
                "WHERE c.ConversationId = @conversationId " +
                "AND c.CreatedUnixTime > @lastSeenMessageTime " +
                "AND c.partitionkey = @partitionKey " +
                "ORDER BY c.CreatedUnixTime DESC";

            var queryDefinition = new QueryDefinition(queryText)
                .WithParameter("@conversationId", conversationId)
                .WithParameter("@lastSeenMessageTime", lastSeenMessageTime)
                .WithParameter("@partitionKey", conversationId);
            var messages = new List<Message>();

            try
            {
                using (var feedIterator = MessageStoreContainer.GetItemQueryIterator<MessageEntity>(
                    queryDefinition,
                    continuationToken: continuationToken,
                     requestOptions: new QueryRequestOptions { MaxItemCount = limit }))
                {
                    var queryResponse = await feedIterator.ReadNextAsync().ConfigureAwait(false);
                    foreach (var messageEntity in queryResponse.Resource)
                    {
                        var message = ToMessage(messageEntity);
                        messages.Add(message);
                    }
                    continuationToken = queryResponse.ContinuationToken;
                }

                if (continuationToken != null)
                {
                    continuationToken = WebUtility.UrlEncode(continuationToken);
                }

                //if (messages.Count == 0) // Add this condition to check if no messages are found
                //{
                //    throw new HttpException("No messages found for the given conversationId.",404); 
                //}
                return new GetConverationMessagesDto(messages, continuationToken);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public static MessageEntity ToEntity(Message message)
        {
            return new MessageEntity(
                id: message.Id,
                partitionkey: message.ConversationId,
                Text: message.Text,
                ConversationId: message.ConversationId,
                CreatedUnixTime: message.CreatedUnixTime,
                SenderId: message.SenderId

                );

        }

        public static Message ToMessage(MessageEntity messageEntity)
        {
            return new Message(
                Id: messageEntity.id,
                Text: messageEntity.Text,
                ConversationId: messageEntity.ConversationId,
                CreatedUnixTime: messageEntity.CreatedUnixTime,
                SenderId: messageEntity.SenderId

                );

        }

        public long UnixTimeNow()
        {
            var timeSpan = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0));
            return (long)timeSpan.TotalSeconds;
        }


    }
    }



