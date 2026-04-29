using CleanArchitecture.Core.DTOs.Chat;
using CleanArchitecture.Core.Entities;
using CleanArchitecture.Core.Interfaces;
using CleanArchitecture.Core.Wrappers;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CleanArchitecture.Core.Features.ClubChannels.Commands.CreateClubChannel
{
    public class CreateClubChannelCommand : IRequest<Response<ClubChannelDto>>
    {
        public int ClubId { get; set; }
        public string CurrentUserId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<int> WriteRoleIds { get; set; } = new List<int>();
    }

    public class CreateClubChannelCommandHandler : IRequestHandler<CreateClubChannelCommand, Response<ClubChannelDto>>
    {
        private readonly IApplicationDbContext _context;

        public CreateClubChannelCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Response<ClubChannelDto>> Handle(CreateClubChannelCommand request, CancellationToken cancellationToken)
        {
            // 1. Verify the user is a leader (President) of this club
            var membership = await _context.UserClubs
                .Include(uc => uc.Role)
                .FirstOrDefaultAsync(uc => uc.UserId == request.CurrentUserId && uc.ClubId == request.ClubId && uc.IsActive, cancellationToken);

            if (membership == null)
                return new Response<ClubChannelDto>("You are not a member of this club.");

            if (membership.Role == null || (membership.Role.Name != "President" && membership.Role.Name != "Vice President"))
                return new Response<ClubChannelDto>("Only club leaders can create channels.");

            // 2. Check for duplicate name
            var exists = await _context.ClubChannels
                .AnyAsync(c => c.ClubId == request.ClubId && c.Name == request.Name, cancellationToken);

            if (exists)
                return new Response<ClubChannelDto>("A channel with this name already exists.");

            // 3. Get max sort order
            var maxSort = await _context.ClubChannels
                .Where(c => c.ClubId == request.ClubId)
                .MaxAsync(c => (int?)c.SortOrder, cancellationToken) ?? 0;

            // 4. Create channel
            var channel = new ClubChannel
            {
                ClubId = request.ClubId,
                Name = request.Name,
                Description = request.Description,
                IsDefault = false,
                SortOrder = maxSort + 1
            };

            _context.ClubChannels.Add(channel);
            await _context.SaveChangesAsync(cancellationToken);

            // 5. Add write roles
            if (request.WriteRoleIds.Any())
            {
                foreach (var roleId in request.WriteRoleIds)
                {
                    _context.ClubChannelWriteRoles.Add(new ClubChannelWriteRole
                    {
                        ChannelId = channel.Id,
                        ClubRoleId = roleId
                    });
                }
                await _context.SaveChangesAsync(cancellationToken);
            }

            // 6. Return DTO
            var roleNames = new List<string>();
            if (request.WriteRoleIds.Any())
            {
                roleNames = await _context.ClubRoles
                    .Where(r => request.WriteRoleIds.Contains(r.Id))
                    .Select(r => r.Name)
                    .ToListAsync(cancellationToken);
            }

            var dto = new ClubChannelDto
            {
                Id = channel.Id,
                ClubId = channel.ClubId,
                Name = channel.Name,
                Description = channel.Description,
                IsDefault = channel.IsDefault,
                SortOrder = channel.SortOrder,
                CanWrite = !request.WriteRoleIds.Any() || request.WriteRoleIds.Contains(membership.ClubRoleId),
                WriteRoleNames = roleNames
            };

            return new Response<ClubChannelDto>(dto);
        }
    }
}
