using CleanArchitecture.Core.DTOs.Chat;
using CleanArchitecture.Core.Entities;
using CleanArchitecture.Core.Interfaces;
using CleanArchitecture.Core.Wrappers;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CleanArchitecture.Core.Features.ClubChannels.Commands.SendChannelMessage
{
    public class SendChannelMessageCommand : IRequest<Response<ChannelMessageDto>>
    {
        public int ClubId { get; set; }
        public int ChannelId { get; set; }
        public string SenderId { get; set; }
        public string Content { get; set; }
    }

    public class SendChannelMessageCommandHandler : IRequestHandler<SendChannelMessageCommand, Response<ChannelMessageDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly INotificationService _notificationService;
        private readonly UserManager<ApplicationUser> _userManager;

        public SendChannelMessageCommandHandler(
            IApplicationDbContext context,
            INotificationService notificationService,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _notificationService = notificationService;
            _userManager = userManager;
        }

        public async Task<Response<ChannelMessageDto>> Handle(SendChannelMessageCommand request, CancellationToken cancellationToken)
        {
            // 1. Verify membership
            var membership = await _context.UserClubs
                .Include(uc => uc.Role)
                .FirstOrDefaultAsync(uc => uc.UserId == request.SenderId && uc.ClubId == request.ClubId && uc.IsActive, cancellationToken);

            if (membership == null)
                return new Response<ChannelMessageDto>("You are not a member of this club.");

            // 2. Verify channel belongs to club
            var channel = await _context.ClubChannels
                .Include(c => c.WriteRoles)
                .FirstOrDefaultAsync(c => c.Id == request.ChannelId && c.ClubId == request.ClubId, cancellationToken);

            if (channel == null)
                return new Response<ChannelMessageDto>("Channel not found.");

            // 3. Check write permission
            if (channel.WriteRoles.Any() && !channel.WriteRoles.Any(wr => wr.ClubRoleId == membership.ClubRoleId))
                return new Response<ChannelMessageDto>("You do not have permission to write in this channel.");

            // 4. Create message
            var sender = await _userManager.FindByIdAsync(request.SenderId);

            var message = new ClubChannelMessage
            {
                ChannelId = request.ChannelId,
                SenderId = request.SenderId,
                Content = request.Content,
                SentAt = DateTime.UtcNow
            };

            _context.ClubChannelMessages.Add(message);
            await _context.SaveChangesAsync(cancellationToken);

            var dto = new ChannelMessageDto
            {
                Id = message.Id,
                ChannelId = message.ChannelId,
                SenderId = message.SenderId,
                SenderName = sender?.FullName ?? "Unknown",
                SenderRoleName = membership.Role?.Name ?? "Member",
                SenderRoleColor = membership.Role?.Color,
                Content = message.Content,
                SentAt = message.SentAt
            };

            // 5. Broadcast to channel group via SignalR
            await _notificationService.BroadcastToChannelAsync(
                request.ClubId,
                request.ChannelId,
                request.SenderId,
                sender?.FullName ?? "Unknown",
                membership.Role?.Name ?? "Member",
                membership.Role?.Color,
                request.Content,
                message.Id.ToString()
            );

            return new Response<ChannelMessageDto>(dto);
        }
    }
}
