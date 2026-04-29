using CleanArchitecture.Core.DTOs.Chat;
using CleanArchitecture.Core.Interfaces;
using CleanArchitecture.Core.Wrappers;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CleanArchitecture.Core.Features.ClubChannels.Queries.GetClubChannels
{
    public class GetClubChannelsQuery : IRequest<Response<List<ClubChannelDto>>>
    {
        public int ClubId { get; set; }
        public string CurrentUserId { get; set; }
    }

    public class GetClubChannelsQueryHandler : IRequestHandler<GetClubChannelsQuery, Response<List<ClubChannelDto>>>
    {
        private readonly IApplicationDbContext _context;

        public GetClubChannelsQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Response<List<ClubChannelDto>>> Handle(GetClubChannelsQuery request, CancellationToken cancellationToken)
        {
            // Verify membership
            var membership = await _context.UserClubs
                .FirstOrDefaultAsync(uc => uc.UserId == request.CurrentUserId && uc.ClubId == request.ClubId && uc.IsActive, cancellationToken);

            if (membership == null)
                return new Response<List<ClubChannelDto>>("You are not a member of this club.");

            var userRoleId = membership.ClubRoleId;

            var channels = await _context.ClubChannels
                .Where(c => c.ClubId == request.ClubId)
                .Include(c => c.WriteRoles)
                    .ThenInclude(wr => wr.ClubRole)
                .OrderBy(c => c.SortOrder)
                .ThenBy(c => c.Name)
                .ToListAsync(cancellationToken);

            var dtos = channels.Select(ch => new ClubChannelDto
            {
                Id = ch.Id,
                ClubId = ch.ClubId,
                Name = ch.Name,
                Description = ch.Description,
                IsDefault = ch.IsDefault,
                SortOrder = ch.SortOrder,
                CanWrite = !ch.WriteRoles.Any() || ch.WriteRoles.Any(wr => wr.ClubRoleId == userRoleId),
                WriteRoleNames = ch.WriteRoles.Select(wr => wr.ClubRole.Name).ToList()
            }).ToList();

            return new Response<List<ClubChannelDto>>(dtos);
        }
    }
}
