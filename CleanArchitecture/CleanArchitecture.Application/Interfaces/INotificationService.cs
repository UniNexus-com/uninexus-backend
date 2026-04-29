using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Core.Interfaces
{
    public interface INotificationService
    {
        Task BroadcastMessageAsync(string title, string message);

        Task BroadcastToGroupAsync(string groupName, string title, string message);
        Task SendDirectMessageAsync(string userId, string title, string content, string messageId);
        Task SendTypingStatusAsync(string userId, string senderId, bool isTyping);
        Task SendReadStatusAsync(string userId, string readerId);
        Task SendReactionAsync(string userId, int messageId, string reaction);
        Task BroadcastToChannelAsync(int clubId, int channelId, string senderId, string senderName, string senderRoleName, string senderRoleColor, string content, string messageId);
    }
}
