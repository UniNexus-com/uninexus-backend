using CleanArchitecture.Core.Entities;
using CleanArchitecture.Core.Exceptions;
using CleanArchitecture.Core.Interfaces;
using CleanArchitecture.Core.Wrappers;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CleanArchitecture.Core.Features.Roles.Commands.DeleteClubRole
{
    public class DeleteClubRoleCommand : IRequest<Response<int>>
    {
        public int Id { get; set; }
    }

    public class DeleteClubRoleCommandHandler : IRequestHandler<DeleteClubRoleCommand, Response<int>>
    {
        private readonly IApplicationDbContext _dbContext;
        private readonly IAuthenticatedUserService _authenticatedUserService;
        private readonly IClubRepositoryAsync _clubRepository;

        public DeleteClubRoleCommandHandler(IApplicationDbContext dbContext, IAuthenticatedUserService authenticatedUserService, IClubRepositoryAsync clubRepository)
        {
            _dbContext = dbContext;
            _authenticatedUserService = authenticatedUserService;
            _clubRepository = clubRepository;
        }

        public async Task<Response<int>> Handle(DeleteClubRoleCommand request, CancellationToken cancellationToken)
        {
            var role = await _dbContext.ClubRoles
                .SingleOrDefaultAsync(r => r.Id == request.Id, cancellationToken);
            
            if (role == null) return new Response<int>("Role not found.");

            if (role.ClubId.HasValue)
            {
                if (!await _clubRepository.HasPrivilegeInClubAsync(role.ClubId.Value, _authenticatedUserService.UserId, "Manage Roles"))
                    throw new ApiException("You do not have permission to manage roles in this club.");
            }

            if (role.IsSystemRole) return new Response<int>("Cannot delete a system role.");

            _dbContext.ClubRoles.Remove(role);
            await _dbContext.SaveChangesAsync(cancellationToken);
            
            return new Response<int>(role.Id);
        }
    }
}
