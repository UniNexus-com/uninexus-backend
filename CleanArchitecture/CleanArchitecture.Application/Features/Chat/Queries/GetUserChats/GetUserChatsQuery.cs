using CleanArchitecture.Core.DTOs.Chat;
using CleanArchitecture.Core.Interfaces;
using CleanArchitecture.Core.Wrappers;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CleanArchitecture.Core.Features.Chat.Queries.GetUserChats
{
    public class GetUserChatsQuery : IRequest<Response<IEnumerable<ChatSummaryDto>>>
    {
        public string UserId { get; set; }
    }

    public class GetUserChatsQueryHandler : IRequestHandler<GetUserChatsQuery, Response<IEnumerable<ChatSummaryDto>>>
    {
        private readonly IApplicationDbContext _context;

        public GetUserChatsQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Response<IEnumerable<ChatSummaryDto>>> Handle(GetUserChatsQuery request, CancellationToken cancellationToken)
        {
            var userId = request.UserId;

            // This is a simplified query to get the last message with each user
            var messages = await _context.ChatMessages
                .Where(m => m.SenderId == userId || m.ReceiverId == userId)
                .OrderByDescending(m => m.SentAt)
                .ToListAsync(cancellationToken);

            var chatSummaries = messages
                .GroupBy(m => m.SenderId == userId ? m.ReceiverId : m.SenderId)
                .Select(g => {
                    var otherUserId = g.Key;
                    var lastMsg = g.First();
                    var otherUser = _context.Set<Entities.ApplicationUser>().FirstOrDefault(u => u.Id == otherUserId);
                    
                    return new ChatSummaryDto
                    {
                        UserId = otherUserId,
                        UserName = otherUser?.FullName ?? "Unknown User",
                        LastMessage = lastMsg.Content,
                        LastMessageTime = lastMsg.SentAt,
                        UnreadCount = g.Count(m => m.ReceiverId == userId && !m.IsRead)
                    };
                })
                .OrderByDescending(s => s.LastMessageTime)
                .ToList();

            return new Response<IEnumerable<ChatSummaryDto>>(chatSummaries);
        }
    }
}
