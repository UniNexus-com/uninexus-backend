using CleanArchitecture.Core.DTOs.Chat;
using CleanArchitecture.Core.Interfaces;
using CleanArchitecture.Core.Wrappers;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CleanArchitecture.Core.Features.ClubChannels.Queries.GetChannelMessages
{
    public class GetChannelMessagesQuery : IRequest<Response<List<ChannelMessageDto>>>
    {
        public int ClubId { get; set; }
        public int ChannelId { get; set; }
        public string CurrentUserId { get; set; }
    }

    public class GetChannelMessagesQueryHandler : IRequestHandler<GetChannelMessagesQuery, Response<List<ChannelMessageDto>>>
    {
        private readonly IApplicationDbContext _context;

        public GetChannelMessagesQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Response<List<ChannelMessageDto>>> Handle(GetChannelMessagesQuery request, CancellationToken cancellationToken)
        {
            // Verify membership
            var isMember = await _context.UserClubs
                .AnyAsync(uc => uc.UserId == request.CurrentUserId && uc.ClubId == request.ClubId && uc.IsActive, cancellationToken);

            if (!isMember)
                return new Response<List<ChannelMessageDto>>("You are not a member of this club.");

            // Verify channel belongs to club
            var channel = await _context.ClubChannels
                .FirstOrDefaultAsync(c => c.Id == request.ChannelId && c.ClubId == request.ClubId, cancellationToken);

            if (channel == null)
                return new Response<List<ChannelMessageDto>>("Channel not found.");

            var messages = await _context.ClubChannelMessages
                .Where(m => m.ChannelId == request.ChannelId)
                .OrderBy(m => m.SentAt)
                .Take(200)
                .Select(m => new ChannelMessageDto
                {
                    Id = m.Id,
                    ChannelId = m.ChannelId,
                    SenderId = m.SenderId,
                    SenderName = m.Sender.FullName,
                    SenderRoleName = _context.UserClubs
                        .Where(uc => uc.UserId == m.SenderId && uc.ClubId == request.ClubId)
                        .Select(uc => uc.Role.Name)
                        .FirstOrDefault() ?? "Member",
                    SenderRoleColor = _context.UserClubs
                        .Where(uc => uc.UserId == m.SenderId && uc.ClubId == request.ClubId)
                        .Select(uc => uc.Role.Color)
                        .FirstOrDefault(),
                    Content = m.Content,
                    SentAt = m.SentAt
                })
                .ToListAsync(cancellationToken);

            return new Response<List<ChannelMessageDto>>(messages);
        }
    }
}
