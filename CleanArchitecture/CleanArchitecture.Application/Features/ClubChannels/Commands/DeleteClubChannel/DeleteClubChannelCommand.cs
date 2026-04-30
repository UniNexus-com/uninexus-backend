using CleanArchitecture.Core.Interfaces;
using CleanArchitecture.Core.Wrappers;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CleanArchitecture.Core.Features.ClubChannels.Commands.DeleteClubChannel
{
    public class DeleteClubChannelCommand : IRequest<Response<bool>>
    {
        public int ClubId       { get; set; }
        public int ChannelId    { get; set; }
        public string CurrentUserId { get; set; }
    }

    public class DeleteClubChannelCommandHandler : IRequestHandler<DeleteClubChannelCommand, Response<bool>>
    {
        private readonly IApplicationDbContext _context;

        public DeleteClubChannelCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Response<bool>> Handle(DeleteClubChannelCommand request, CancellationToken cancellationToken)
        {
            // 1. Verify the user is President / Vice President
            var membership = await _context.UserClubs
                .Include(uc => uc.Role)
                .FirstOrDefaultAsync(uc =>
                    uc.UserId == request.CurrentUserId &&
                    uc.ClubId == request.ClubId &&
                    uc.IsActive, cancellationToken);

            if (membership == null)
                return new Response<bool>("You are not a member of this club.");

            if (membership.Role == null ||
                (membership.Role.Name != "President" && membership.Role.Name != "Vice President"))
                return new Response<bool>("Only club leaders can delete channels.");

            // 2. Find channel
            var channel = await _context.ClubChannels
                .FirstOrDefaultAsync(c => c.Id == request.ChannelId && c.ClubId == request.ClubId, cancellationToken);

            if (channel == null)
                return new Response<bool>("Channel not found.");

            if (channel.IsDefault)
                return new Response<bool>("Default channel cannot be deleted.");

            // 3. Remove write roles
            var writeRoles = _context.ClubChannelWriteRoles.Where(r => r.ChannelId == channel.Id);
            _context.ClubChannelWriteRoles.RemoveRange(writeRoles);

            // 4. Remove messages
            var messages = _context.ClubChannelMessages.Where(m => m.ChannelId == channel.Id);
            _context.ClubChannelMessages.RemoveRange(messages);

            // 5. Remove channel
            _context.ClubChannels.Remove(channel);

            await _context.SaveChangesAsync(cancellationToken);

            return new Response<bool>(true, "Channel deleted successfully.");
        }
    }
}
