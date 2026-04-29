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

        public async Task BroadcastToGroupAsync(string groupName, string title, string message)
        {
            await _hubContext.Clients.Group(groupName).SendAsync("ReceiveNotification", title, message);
        }

        public async Task SendDirectMessageAsync(string userId, string title, string content, string messageId)
        {
            await _hubContext.Clients.User(userId).SendAsync("ReceiveDirectMessage", title, content, messageId);
        }

        public async Task SendTypingStatusAsync(string userId, string senderId, bool isTyping)
        {
            await _hubContext.Clients.User(userId).SendAsync("UserTyping", senderId, isTyping);
        }

        public async Task SendReadStatusAsync(string userId, string readerId)
        {
            await _hubContext.Clients.User(userId).SendAsync("MessagesRead", readerId);
        }

        public async Task SendReactionAsync(string userId, int messageId, string reaction)
        {
            await _hubContext.Clients.User(userId).SendAsync("MessageReacted", messageId, reaction);
        }

        public async Task BroadcastToChannelAsync(int clubId, int channelId, string senderId, string senderName, string senderRoleName, string senderRoleColor, string content, string messageId)
        {
            await _hubContext.Clients.Group(clubId.ToString()).SendAsync(
                "ReceiveChannelMessage",
                clubId, channelId, senderId, senderName, senderRoleName, senderRoleColor, content, messageId);
        }
    }
}
