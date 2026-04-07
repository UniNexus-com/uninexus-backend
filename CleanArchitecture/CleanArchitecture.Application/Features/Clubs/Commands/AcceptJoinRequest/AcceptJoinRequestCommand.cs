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

        public AcceptJoinRequestCommandHandler(
            IClubRepositoryAsync clubRepository, 
            IGenericRepositoryAsync<UserClub> userClubRepository,
            IAuthenticatedUserService authenticatedUserService)
        {
            _clubRepository = clubRepository;
            _userClubRepository = userClubRepository;
            _authenticatedUserService = authenticatedUserService;
        }

        public async Task<Response<int>> Handle(AcceptJoinRequestCommand request, CancellationToken cancellationToken)
        {
            var joinRequest = await _clubRepository.GetJoinRequestByIdAsync(request.Id);
            if (joinRequest == null) throw new ApiException("Join request not found.");
            if (joinRequest.Status != ClubJoinStatus.Pending) throw new ApiException("Request already processed.");

            // 1. Update Request Status
            joinRequest.Status = ClubJoinStatus.Approved;
            joinRequest.ProcessedBy = _authenticatedUserService.UserId;
            joinRequest.ProcessedDate = DateTime.UtcNow;
            await _clubRepository.UpdateJoinRequestAsync(joinRequest);
            
            // 2. Add User to Club
            var userClub = new UserClub
            {
                UserId = joinRequest.UserId,
                ClubId = joinRequest.ClubId,
                JoinDate = DateTime.UtcNow,
                IsActive = true
                // club_role_id will be set by the DB trigger
            };
            await _userClubRepository.AddAsync(userClub);

            return new Response<int>(joinRequest.Id, "Member accepted successfully.");
        }
    }
}
