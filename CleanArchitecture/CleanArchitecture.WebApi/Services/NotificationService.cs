using CleanArchitecture.Core.Interfaces;
using CleanArchitecture.WebApi.Hubs;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace CleanArchitecture.WebApi.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationService(IHubContext<NotificationHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task BroadcastMessageAsync(string title, string message)
        {
            await _hubContext.Clients.All.SendAsync("ReceiveNotification", title, message);
        }
    }
}
