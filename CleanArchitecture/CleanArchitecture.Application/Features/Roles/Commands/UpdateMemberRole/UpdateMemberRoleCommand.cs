using CleanArchitecture.Core.Entities;
using CleanArchitecture.Core.Exceptions;
using CleanArchitecture.Core.Interfaces;
using CleanArchitecture.Core.Wrappers;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CleanArchitecture.Core.Features.Roles.Commands.UpdateMemberRole
{
    public class UpdateMemberRoleCommand : IRequest<Response<string>>
    {
        public int ClubId { get; set; }
        public string UserId { get; set; }
        public int RoleId { get; set; }
    }

    public class UpdateMemberRoleCommandHandler : IRequestHandler<UpdateMemberRoleCommand, Response<string>>
    {
        private readonly IApplicationDbContext _dbContext;
        private readonly IAuthenticatedUserService _authenticatedUserService;
        private readonly IClubRepositoryAsync _clubRepository;

        public UpdateMemberRoleCommandHandler(IApplicationDbContext dbContext, IAuthenticatedUserService authenticatedUserService, IClubRepositoryAsync clubRepository)
        {
            _dbContext = dbContext;
            _authenticatedUserService = authenticatedUserService;
            _clubRepository = clubRepository;
        }

        public async Task<Response<string>> Handle(UpdateMemberRoleCommand request, CancellationToken cancellationToken)
        {
            if (!await _clubRepository.HasPrivilegeInClubAsync(request.ClubId, _authenticatedUserService.UserId, "Manage Members"))
                throw new ApiException("You do not have permission to manage members in this club.");

            if (request.UserId == _authenticatedUserService.UserId)
            {
                return new Response<string>("You cannot change your own role and permissions.");
            }

            var membership = await _dbContext.UserClubs
                .Include(uc => uc.Role)
                .AsTracking()
                .SingleOrDefaultAsync(uc => uc.UserId == request.UserId && uc.ClubId == request.ClubId, cancellationToken);
            
            if (membership == null) return new Response<string>("Membership not found.");
            
            // Prevent changing the role of a President/Leader via this command if necessary
            if (membership.Role != null && membership.Role.IsSystemRole && membership.Role.Name == "President")
            {
                return new Response<string>("Community leader's role cannot be changed.");
            }

            var targetRole = await _dbContext.ClubRoles.FindAsync(request.RoleId);
            if (targetRole != null && targetRole.IsSystemRole && targetRole.Name == "President")
                return new Response<string>("President role can only be assigned by an administrator.");

            membership.ClubRoleId = request.RoleId;
            
            _dbContext.UserClubs.Update(membership);
            await _dbContext.SaveChangesAsync(cancellationToken);
            
            return new Response<string>(request.UserId, "Role updated successfully.");
        }
    }
}
