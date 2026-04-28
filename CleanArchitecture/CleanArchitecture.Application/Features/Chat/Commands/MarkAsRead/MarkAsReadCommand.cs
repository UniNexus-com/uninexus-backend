using CleanArchitecture.Core.Interfaces;
using CleanArchitecture.Core.Wrappers;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CleanArchitecture.Core.Features.Chat.Commands.MarkAsRead
{
    public class MarkAsReadCommand : IRequest<Response<bool>>
    {
        public string CurrentUserId { get; set; }
        public string SenderId { get; set; }
    }

    public class MarkAsReadCommandHandler : IRequestHandler<MarkAsReadCommand, Response<bool>>
    {
        private readonly IApplicationDbContext _context;
        private readonly INotificationService _notificationService;

        public MarkAsReadCommandHandler(IApplicationDbContext context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        public async Task<Response<bool>> Handle(MarkAsReadCommand request, CancellationToken cancellationToken)
        {
            var unreadMessages = await _context.ChatMessages
                .Where(m => m.SenderId == request.SenderId && m.ReceiverId == request.CurrentUserId && !m.IsRead)
                .ToListAsync(cancellationToken);

            if (unreadMessages.Any())
            {
                foreach (var msg in unreadMessages)
                {
                    msg.IsRead = true;
                }
                await _context.SaveChangesAsync(cancellationToken);

                // Notify the sender that their messages were read
                await _notificationService.SendReadStatusAsync(request.SenderId, request.CurrentUserId);
            }

            return new Response<bool>(true);
        }
    }
}
