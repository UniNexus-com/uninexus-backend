using CleanArchitecture.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace CleanArchitecture.WebApi.Hubs
{
    [Authorize]
    public class NotificationHub : Hub
    {
        private readonly IApplicationDbContext _context;

        public NotificationHub(IApplicationDbContext context)
        {
            _context = context;
        }
        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.Claims.FirstOrDefault(c => c.Type == "uid")?.Value;

            if (!string.IsNullOrEmpty(userId))
            {
                var userClubs = await _context.UserClubs
                    .Where(c => c.UserId == userId)
                    .Select(uc => uc.ClubId)
                    .ToListAsync();

                foreach (var clubId in userClubs)
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, clubId.ToString());
                }
            }
            
            await base.OnConnectedAsync();
        }

        public async Task SendTypingStatus(string receiverId, bool isTyping)
        {
            var senderId = Context.User?.Claims.FirstOrDefault(c => c.Type == "uid")?.Value;
            if (!string.IsNullOrEmpty(senderId))
            {
                await Clients.User(receiverId).SendAsync("UserTyping", senderId, isTyping);
            }
        }

        public override async Task OnDisconnectedAsync(System.Exception exception)
        {
            await base.OnDisconnectedAsync(exception);
        }
    }
}
