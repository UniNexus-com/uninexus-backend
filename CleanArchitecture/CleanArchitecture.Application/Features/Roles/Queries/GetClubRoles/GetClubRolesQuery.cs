using CleanArchitecture.Core.DTOs.Roles;
using CleanArchitecture.Core.Entities;
using CleanArchitecture.Core.Wrappers;
using CleanArchitecture.Core.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CleanArchitecture.Core.Features.Roles.Queries.GetClubRoles
{
    public class GetClubRolesQuery : IRequest<Response<IEnumerable<ClubRoleViewModel>>>
    {
        public int ClubId { get; set; }
    }

    public class GetClubRolesQueryHandler : IRequestHandler<GetClubRolesQuery, Response<IEnumerable<ClubRoleViewModel>>>
    {
        private readonly IApplicationDbContext _dbContext;

        public GetClubRolesQueryHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Response<IEnumerable<ClubRoleViewModel>>> Handle(GetClubRolesQuery request, CancellationToken cancellationToken)
        {
            var roles = await _dbContext.Set<ClubRole>()
                .Include(r => r.RolePrivileges)
                .Where(r => r.IsSystemRole || r.ClubId == request.ClubId)
                .ToListAsync(cancellationToken);

            System.Console.WriteLine($"[DIAGNOSTIC] GetClubRolesQuery - ClubId: {request.ClubId}, Count: {roles.Count}");

            var result = roles.Select(r => new ClubRoleViewModel
            {
                Id = r.Id,
                Name = r.Name,
                Description = r.Description,
                Color = r.Color,
                IsSystemRole = r.IsSystemRole,
                ClubId = r.ClubId,
                PrivilegeIds = r.RolePrivileges.Select(rp => rp.PrivilegeId).ToList()
            });

            return new Response<IEnumerable<ClubRoleViewModel>>(result);
        }
    }
}
