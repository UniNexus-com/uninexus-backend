using CleanArchitecture.Core.DTOs.Chat;
using CleanArchitecture.Core.Entities;
using CleanArchitecture.Core.Interfaces;
using CleanArchitecture.Core.Wrappers;
using MediatR;
using Microsoft.AspNetCore.Identity;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CleanArchitecture.Core.Features.Chat.Commands.SendMessage
{
    public class SendMessageCommand : IRequest<Response<ChatMessageDto>>
    {
        public string SenderId { get; set; }
        public string ReceiverId { get; set; }
        public string Content { get; set; }
    }

    public class SendMessageCommandHandler : IRequestHandler<SendMessageCommand, Response<ChatMessageDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly INotificationService _notificationService;
        private readonly UserManager<ApplicationUser> _userManager;

        public SendMessageCommandHandler(IApplicationDbContext context, INotificationService notificationService, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _notificationService = notificationService;
            _userManager = userManager;
        }

        public async Task<Response<ChatMessageDto>> Handle(SendMessageCommand request, CancellationToken cancellationToken)
        {
            var sender = await _userManager.FindByIdAsync(request.SenderId);
            var receiver = await _userManager.FindByIdAsync(request.ReceiverId);

            if (receiver == null) return new Response<ChatMessageDto>("Receiver not found.");

            var message = new ChatMessage
            {
                SenderId = request.SenderId,
                ReceiverId = request.ReceiverId,
                Content = request.Content,
                SentAt = DateTime.UtcNow,
                IsRead = false
            };

            _context.ChatMessages.Add(message);
            await _context.SaveChangesAsync(cancellationToken);

            var dto = new ChatMessageDto
            {
                Id = message.Id,
                SenderId = message.SenderId,
                SenderName = sender.FullName,
                ReceiverId = message.ReceiverId,
                ReceiverName = receiver.FullName,
                Content = message.Content,
                IsRead = message.IsRead,
                SentAt = message.SentAt
            };

            // Notify receiver via SignalR — senderId allows Flutter to match the open chat
            await _notificationService.SendDirectMessageAsync(request.ReceiverId, request.SenderId, sender.FullName, request.Content, message.Id.ToString());

            return new Response<ChatMessageDto>(dto);
        }
    }
}
