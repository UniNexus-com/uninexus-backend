using CleanArchitecture.Core.Interfaces;
using CleanArchitecture.Core.Wrappers;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace CleanArchitecture.Core.Features.Chat.Commands.ReactToMessage
{
    public class ReactToMessageCommand : IRequest<Response<bool>>
    {
        public int MessageId { get; set; }
        public string Reaction { get; set; }
        public string CurrentUserId { get; set; }
    }

    public class ReactToMessageCommandHandler : IRequestHandler<ReactToMessageCommand, Response<bool>>
    {
        private readonly IApplicationDbContext _context;
        private readonly INotificationService _notificationService;

        public ReactToMessageCommandHandler(IApplicationDbContext context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        public async Task<Response<bool>> Handle(ReactToMessageCommand request, CancellationToken cancellationToken)
        {
            var message = await _context.ChatMessages.FindAsync(request.MessageId);
            if (message == null) return new Response<bool>("Message not found.");

            message.Reaction = request.Reaction;
            await _context.SaveChangesAsync(cancellationToken);

            // Notify the other participant
            var otherUserId = message.SenderId == request.CurrentUserId ? message.ReceiverId : message.SenderId;
            await _notificationService.SendReactionAsync(otherUserId, request.MessageId, request.Reaction);

            return new Response<bool>(true);
        }
    }
}
