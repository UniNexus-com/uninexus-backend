using CleanArchitecture.Core.DTOs.Chat;
using CleanArchitecture.Core.Interfaces;
using CleanArchitecture.Core.Wrappers;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CleanArchitecture.Core.Features.ClubChannels.Commands.UpdateClubChannel
{
    public class UpdateClubChannelCommand : IRequest<Response<ClubChannelDto>>
    {
        public int ClubId           { get; set; }
        public int ChannelId        { get; set; }
        public string CurrentUserId { get; set; }
        public string Name          { get; set; }
        public string Description   { get; set; }
        public List<int> WriteRoleIds      { get; set; } = new List<int>();
        public List<int> VisibilityRoleIds { get; set; } = new List<int>();
    }

    public class UpdateClubChannelCommandHandler : IRequestHandler<UpdateClubChannelCommand, Response<ClubChannelDto>>
    {
        private readonly IApplicationDbContext _context;

        public UpdateClubChannelCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Response<ClubChannelDto>> Handle(UpdateClubChannelCommand request, CancellationToken cancellationToken)
        {
            // 1. Verify user is President / Vice President
            var membership = await _context.UserClubs
                .Include(uc => uc.Role)
                .FirstOrDefaultAsync(uc =>
                    uc.UserId == request.CurrentUserId &&
                    uc.ClubId == request.ClubId &&
                    uc.IsActive, cancellationToken);

            if (membership == null)
                return new Response<ClubChannelDto>("You are not a member of this club.");

            if (membership.Role == null ||
                (membership.Role.Name != "President" && membership.Role.Name != "Vice President"))
                return new Response<ClubChannelDto>("Only club leaders can edit channels.");

            // 2. Find channel
            var channel = await _context.ClubChannels
                .FirstOrDefaultAsync(c => c.Id == request.ChannelId && c.ClubId == request.ClubId, cancellationToken);

            if (channel == null)
                return new Response<ClubChannelDto>("Channel not found.");

            // 3. Update basic fields
            if (!string.IsNullOrWhiteSpace(request.Name))
                channel.Name = request.Name.Trim();
            channel.Description = request.Description?.Trim() ?? string.Empty;
            await _context.SaveChangesAsync(cancellationToken);

            // 4. Update write roles
            var existingWrite = _context.ClubChannelWriteRoles.Where(r => r.ChannelId == channel.Id);
            _context.ClubChannelWriteRoles.RemoveRange(existingWrite);
            await _context.SaveChangesAsync(cancellationToken);

            foreach (var roleId in request.WriteRoleIds)
            {
                _context.ClubChannelWriteRoles.Add(new Core.Entities.ClubChannelWriteRole
                {
                    ChannelId = channel.Id,
                    ClubRoleId = roleId
                });
            }

            // 5. Update visibility roles
            var existingVis = _context.ClubChannelVisibilityRoles.Where(r => r.ChannelId == channel.Id);
            _context.ClubChannelVisibilityRoles.RemoveRange(existingVis);
            await _context.SaveChangesAsync(cancellationToken);

            foreach (var roleId in request.VisibilityRoleIds)
            {
                _context.ClubChannelVisibilityRoles.Add(new Core.Entities.ClubChannelVisibilityRole
                {
                    ChannelId = channel.Id,
                    ClubRoleId = roleId
                });
            }

            await _context.SaveChangesAsync(cancellationToken);

            // 6. Build return DTO with role names
            var writeRoleNames = request.WriteRoleIds.Any()
                ? await _context.ClubRoles
                    .Where(r => request.WriteRoleIds.Contains(r.Id))
                    .Select(r => r.Name)
                    .ToListAsync(cancellationToken)
                : new List<string>();

            var visRoleNames = request.VisibilityRoleIds.Any()
                ? await _context.ClubRoles
                    .Where(r => request.VisibilityRoleIds.Contains(r.Id))
                    .Select(r => r.Name)
                    .ToListAsync(cancellationToken)
                : new List<string>();

            var dto = new ClubChannelDto
            {
                Id                  = channel.Id,
                ClubId              = channel.ClubId,
                Name                = channel.Name,
                Description         = channel.Description,
                IsDefault           = channel.IsDefault,
                SortOrder           = channel.SortOrder,
                CanWrite            = !request.WriteRoleIds.Any() || request.WriteRoleIds.Contains(membership.ClubRoleId),
                IsVisible           = !request.VisibilityRoleIds.Any() || request.VisibilityRoleIds.Contains(membership.ClubRoleId),
                WriteRoleNames      = writeRoleNames,
                WriteRoleIds        = request.WriteRoleIds,
                VisibilityRoleNames = visRoleNames,
                VisibilityRoleIds   = request.VisibilityRoleIds,
            };

            return new Response<ClubChannelDto>(dto, "Channel updated successfully.");
        }
    }
}
