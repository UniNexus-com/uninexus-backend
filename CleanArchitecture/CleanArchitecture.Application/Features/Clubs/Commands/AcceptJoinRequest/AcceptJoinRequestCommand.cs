using CleanArchitecture.Core.Entities;
using CleanArchitecture.Core.Enums;
using CleanArchitecture.Core.Exceptions;
using CleanArchitecture.Core.Interfaces;
using CleanArchitecture.Core.Wrappers;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CleanArchitecture.Core.Features.Clubs.Commands.AcceptJoinRequest
{
    public class AcceptJoinRequestCommand : IRequest<Response<int>>
    {
        public int Id { get; set; }
    }

    public class AcceptJoinRequestCommandHandler : IRequestHandler<AcceptJoinRequestCommand, Response<int>>
    {
        private readonly IClubRepositoryAsync _clubRepository;
        private readonly IGenericRepositoryAsync<UserClub> _userClubRepository;
        private readonly IAuthenticatedUserService _authenticatedUserService;
        private readonly IApplicationDbContext _context;

        public AcceptJoinRequestCommandHandler(
            IClubRepositoryAsync clubRepository, 
            IGenericRepositoryAsync<UserClub> userClubRepository,
            IAuthenticatedUserService authenticatedUserService,
            IApplicationDbContext context)
        {
            _clubRepository = clubRepository;
            _userClubRepository = userClubRepository;
            _authenticatedUserService = authenticatedUserService;
            _context = context;
        }

        public async Task<Response<int>> Handle(AcceptJoinRequestCommand request, CancellationToken cancellationToken)
        {
            var joinRequest = await _clubRepository.GetJoinRequestByIdAsync(request.Id);
            if (joinRequest == null) throw new ApiException("Join request not found.");

            if (!await _clubRepository.HasPrivilegeInClubAsync(joinRequest.ClubId, _authenticatedUserService.UserId, "Manage Members"))
                throw new ApiException("You do not have permission to manage members in this club.");

            if (joinRequest.Status != ClubJoinStatus.Pending) throw new ApiException("Request already processed.");

            // 1. Update Request Status
            joinRequest.Status = ClubJoinStatus.Approved;
            joinRequest.ProcessedBy = _authenticatedUserService.UserId;
            joinRequest.ProcessedDate = DateTime.UtcNow;
            await _clubRepository.UpdateJoinRequestAsync(joinRequest);
            
            // 2. Guard: user must not already be a member
            if (await _clubRepository.IsClubMemberAsync(joinRequest.ClubId, joinRequest.UserId))
                throw new ApiException("User is already a member of this club.");

            // 3. Add User to Club
            var userClub = new UserClub
            {
                UserId = joinRequest.UserId,
                ClubId = joinRequest.ClubId,
                JoinDate = DateTime.UtcNow,
                IsActive = true
                // club_role_id will be set by the DB trigger
            };
            await _userClubRepository.AddAsync(userClub);

            var user = await _context.Set<ApplicationUser>().FindAsync(new object[] { joinRequest.UserId }, cancellationToken);
            if (user != null)
            {
                user.ScoreWalletBalance += 350;
                user.TotalScore += 350;
                _context.Set<ApplicationUser>().Update(user);
                await _context.SaveChangesAsync(cancellationToken);
            }

            return new Response<int>(joinRequest.Id, "Member accepted successfully.");
        }
    }
}
