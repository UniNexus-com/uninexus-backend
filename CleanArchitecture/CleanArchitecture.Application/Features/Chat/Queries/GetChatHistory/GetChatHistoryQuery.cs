using CleanArchitecture.Core.DTOs.Chat;
using CleanArchitecture.Core.Interfaces;
using CleanArchitecture.Core.Wrappers;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CleanArchitecture.Core.Features.Chat.Queries.GetChatHistory
{
    public class GetChatHistoryQuery : IRequest<Response<IEnumerable<ChatMessageDto>>>
    {
        public string CurrentUserId { get; set; }
        public string TargetUserId { get; set; }
    }

    public class GetChatHistoryQueryHandler : IRequestHandler<GetChatHistoryQuery, Response<IEnumerable<ChatMessageDto>>>
    {
        private readonly IApplicationDbContext _context;

        public GetChatHistoryQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Response<IEnumerable<ChatMessageDto>>> Handle(GetChatHistoryQuery request, CancellationToken cancellationToken)
        {
            var messages = await _context.ChatMessages
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .Where(m => (m.SenderId == request.CurrentUserId && m.ReceiverId == request.TargetUserId) ||
                            (m.SenderId == request.TargetUserId && m.ReceiverId == request.CurrentUserId))
                .OrderBy(m => m.SentAt)
                .Select(m => new ChatMessageDto
                {
                    Id = m.Id,
                    SenderId = m.SenderId,
                    SenderName = m.Sender.FullName,
                    ReceiverId = m.ReceiverId,
                    ReceiverName = m.Receiver.FullName,
                    Content = m.Content,
                    IsRead = m.IsRead,
                    SentAt = m.SentAt,
                    Reaction = m.Reaction
                })
                .ToListAsync(cancellationToken);

            return new Response<IEnumerable<ChatMessageDto>>(messages);
        }
    }
}
