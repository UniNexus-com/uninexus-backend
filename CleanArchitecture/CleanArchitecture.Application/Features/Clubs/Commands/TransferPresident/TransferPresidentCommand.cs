using CleanArchitecture.Core.Entities;
using CleanArchitecture.Core.Exceptions;
using CleanArchitecture.Core.Interfaces;
using CleanArchitecture.Core.Wrappers;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CleanArchitecture.Core.Features.Clubs.Commands.TransferPresident
{
    public class TransferPresidentCommand : IRequest<Response<string>>
    {
        public int ClubId { get; set; }
        public string NewPresidentUserId { get; set; }
    }

    public class TransferPresidentCommandHandler : IRequestHandler<TransferPresidentCommand, Response<string>>
    {
        private readonly IApplicationDbContext _dbContext;

        public TransferPresidentCommandHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Response<string>> Handle(TransferPresidentCommand request, CancellationToken cancellationToken)
        {
            var presidentRole = await _dbContext.ClubRoles
                .FirstOrDefaultAsync(r => r.Name == "President" && r.IsSystemRole, cancellationToken);
            if (presidentRole == null) throw new ApiException("President role not found.");

            var activeMemberRole = await _dbContext.ClubRoles
                .FirstOrDefaultAsync(r => r.Name == "Active Member" && r.IsSystemRole, cancellationToken);
            if (activeMemberRole == null) throw new ApiException("Active Member role not found.");

            var newPresidentMembership = await _dbContext.UserClubs
                .FirstOrDefaultAsync(uc => uc.ClubId == request.ClubId && uc.UserId == request.NewPresidentUserId, cancellationToken);
            if (newPresidentMembership == null) throw new ApiException("User is not a member of this club.");

            var currentPresident = await _dbContext.UserClubs
                .FirstOrDefaultAsync(uc => uc.ClubId == request.ClubId && uc.ClubRoleId == presidentRole.Id, cancellationToken);

            if (currentPresident != null)
            {
                currentPresident.ClubRoleId = activeMemberRole.Id;
                _dbContext.UserClubs.Update(currentPresident);
            }

            newPresidentMembership.ClubRoleId = presidentRole.Id;
            _dbContext.UserClubs.Update(newPresidentMembership);

            var newPresidentUser = await _dbContext.Set<ApplicationUser>().FindAsync(new object[] { request.NewPresidentUserId }, cancellationToken);
            if (newPresidentUser != null)
            {
                newPresidentUser.ScoreWalletBalance += 1000;
                newPresidentUser.TotalScore += 1000;
                _dbContext.Set<ApplicationUser>().Update(newPresidentUser);
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            return new Response<string>(request.NewPresidentUserId, "President transferred successfully.");
        }
    }
}
