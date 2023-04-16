using ChatService.Web.Dtos;
using ChatService.Web.Storage.Entities;
using Microsoft.Azure.Cosmos;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Mvc;
using Azure.Storage.Blobs.Models;
using System.Net;

namespace ChatService.Web.Storage
{




    public class CosmosProfileStore : IProfileStore
    {


        private readonly CosmosClient _cosmosclient;


        private Container ProfileContainer => _cosmosclient.GetDatabase("ChatService").GetContainer("profile");



        public CosmosProfileStore(CosmosClient cosmosclient)
        {

            _cosmosclient = cosmosclient;


        }



        public async Task UpsertProfile(Profile? profile)
        {
            if (profile == null ||
                string.IsNullOrWhiteSpace(profile.Username) ||
                string.IsNullOrWhiteSpace(profile.FirstName) ||
                string.IsNullOrWhiteSpace(profile.LastName) 
            
                )
            {
                throw new ArgumentException($"Invalid profile {profile}", nameof(profile));
            }


            try
            {
                await ProfileContainer.UpsertItemAsync(ToEntity(profile));
            }

            catch
            {

                throw;

            }

        }

        public async Task<Profile?> GetProfile(string? username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentException($"The {username} parameter cannot be null or empty.");
            }

            try
            {
                var entity = await ProfileContainer.ReadItemAsync<ProfileEntity>(

                        id: username,
                        partitionKey: new PartitionKey(username),
                            new ItemRequestOptions
                            {
                                ConsistencyLevel = ConsistencyLevel.Session
                            }

                    );
                return ToProfile(entity);
            }

            catch (CosmosException ex)
            {
                if (ex.StatusCode == HttpStatusCode.NotFound)
                {
                    return null;
                }

                throw;

            }

        }




        public static ProfileEntity ToEntity(Profile profile)
        {

            return new ProfileEntity(
                id: profile.Username,
                partitionkey: profile.Username,
                FirstName: profile.FirstName,
                LastName: profile.LastName,
                ProfilePictureid: profile.ProfilePictureid
                );

        }

        public static Profile ToProfile(ProfileEntity entity)
        {
            return new Profile(
                Username: entity.id,
                FirstName: entity.FirstName,
                LastName: entity.LastName,
                ProfilePictureid: entity.ProfilePictureid
                );
        }










}
}