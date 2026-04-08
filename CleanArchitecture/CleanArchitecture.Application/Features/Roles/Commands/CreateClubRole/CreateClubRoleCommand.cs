using AutoMapper;
using CleanArchitecture.Core.Entities;
using CleanArchitecture.Core.Interfaces;
using CleanArchitecture.Core.Wrappers;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CleanArchitecture.Core.Features.Roles.Commands.CreateClubRole
{
    public class CreateClubRoleCommand : IRequest<Response<int>>
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Color { get; set; }
        public int? ClubId { get; set; }
        public List<int> PrivilegeIds { get; set; } = new List<int>();
    }

    public class CreateClubRoleCommandHandler : IRequestHandler<CreateClubRoleCommand, Response<int>>
    {
        private readonly IApplicationDbContext _dbContext;
        private readonly IMapper _mapper;

        public CreateClubRoleCommandHandler(IApplicationDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task<Response<int>> Handle(CreateClubRoleCommand request, CancellationToken cancellationToken)
        {
            // Validation: Check if a role with the same name already exists in this club
            var existingRole = await _dbContext.ClubRoles
                .Where(r => r.ClubId == request.ClubId && r.Name.ToLower() == request.Name.ToLower())
                .AnyAsync(cancellationToken);

            if (existingRole)
            {
                return new Response<int>($"A role with the name '{request.Name}' already exists in this club.");
            }

            var role = _mapper.Map<ClubRole>(request);
            role.IsSystemRole = false;

            // Add role privileges
            if (request.PrivilegeIds != null && request.PrivilegeIds.Any())
            {
                foreach (var privilegeId in request.PrivilegeIds)
                {
                    role.RolePrivileges.Add(new ClubRolePrivilege
                    {
                        PrivilegeId = privilegeId
                    });
                }
            }

            _dbContext.ClubRoles.Add(role);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return new Response<int>(role.Id);
        }
    }
}
