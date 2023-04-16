using ChatService.Web.Dtos;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace ChatService.Web.Storage
{
    public interface IProfileStore
    {
        Task<Profile?> GetProfile(string? username);
        Task UpsertProfile (Profile? profile);




    }
}
