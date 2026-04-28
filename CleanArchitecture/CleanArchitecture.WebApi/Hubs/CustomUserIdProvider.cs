using Microsoft.AspNetCore.SignalR;
using System.Linq;

namespace CleanArchitecture.WebApi.Hubs
{
    public class CustomUserIdProvider : IUserIdProvider
    {
        public string GetUserId(HubConnectionContext connection)
        {
            return connection.User?.Claims.FirstOrDefault(c => c.Type == "uid")?.Value;
        }
    }
}
