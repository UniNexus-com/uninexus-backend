using CleanArchitecture.Core.Entities;
using CleanArchitecture.Core.Exceptions;
using CleanArchitecture.Core.Interfaces;
using CleanArchitecture.Core.Wrappers;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CleanArchitecture.Core.Features.Roles.Commands.UpdateClubRole
{
    public class UpdateClubRoleCommand : IRequest<Response<int>>
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Color { get; set; }
        public int? ClubId { get; set; }
        public List<int> PrivilegeIds { get; set; } = new List<int>();
    }

    public class UpdateClubRoleCommandHandler : IRequestHandler<UpdateClubRoleCommand, Response<int>>
    {
        private readonly IApplicationDbContext _dbContext;
        private readonly IAuthenticatedUserService _authenticatedUserService;
        private readonly IClubRepositoryAsync _clubRepository;

        public UpdateClubRoleCommandHandler(IApplicationDbContext dbContext, IAuthenticatedUserService authenticatedUserService, IClubRepositoryAsync clubRepository)
        {
            _dbContext = dbContext;
            _authenticatedUserService = authenticatedUserService;
            _clubRepository = clubRepository;
        }

        public async Task<Response<int>> Handle(UpdateClubRoleCommand request, CancellationToken cancellationToken)
        {
            var role = await _dbContext.ClubRoles
                .Include(r => r.RolePrivileges)
                .SingleOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

            if (role == null) return new Response<int>("Role not found.");

            if (role.IsSystemRole) return new Response<int>("Cannot edit a system role.");

            var clubId = request.ClubId ?? role.ClubId;
            if (clubId.HasValue)
            {
                if (!await _clubRepository.HasPrivilegeInClubAsync(clubId.Value, _authenticatedUserService.UserId, "Manage Roles"))
                    throw new ApiException("You do not have permission to manage roles in this club.");
            }

            var duplicate = await _dbContext.ClubRoles
                .AnyAsync(r => r.Id != request.Id && r.ClubId == role.ClubId && r.Name.ToLower() == request.Name.ToLower(), cancellationToken);

            if (duplicate)
                return new Response<int>($"A role with the name '{request.Name}' already exists in this club.");

            role.Name = request.Name;
            role.Description = request.Description;
            role.Color = request.Color;

            var existing = await _dbContext.ClubRolePrivileges
                .Where(rp => rp.ClubRoleId == role.Id)
                .ToListAsync(cancellationToken);
            _dbContext.ClubRolePrivileges.RemoveRange(existing);

            if (request.PrivilegeIds != null && request.PrivilegeIds.Any())
            {
                foreach (var privilegeId in request.PrivilegeIds)
                {
                    _dbContext.ClubRolePrivileges.Add(new ClubRolePrivilege { ClubRoleId = role.Id, PrivilegeId = privilegeId });
                }
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            return new Response<int>(role.Id);
        }
    }
}
