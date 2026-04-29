using CleanArchitecture.Core.Entities;
using CleanArchitecture.Core.Enums;
using CleanArchitecture.Core.Exceptions;
using CleanArchitecture.Core.Interfaces;
using CleanArchitecture.Core.Wrappers;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CleanArchitecture.Core.Features.Clubs.Commands.SendJoinRequest
{
    public class SendJoinRequestCommand : IRequest<Response<int>>
    {
        public int ClubId { get; set; }
    }

    public class SendJoinRequestCommandHandler : IRequestHandler<SendJoinRequestCommand, Response<int>>
    {
        private readonly IClubRepositoryAsync _clubRepository;
        private readonly IAuthenticatedUserService _authenticatedUserService;

        public SendJoinRequestCommandHandler(
            IClubRepositoryAsync clubRepository,
            IAuthenticatedUserService authenticatedUserService)
        {
            _clubRepository = clubRepository;
            _authenticatedUserService = authenticatedUserService;
        }

        public async Task<Response<int>> Handle(SendJoinRequestCommand request, CancellationToken cancellationToken)
        {
            var userId = _authenticatedUserService.UserId;

            // Check if already a member
            if (await _clubRepository.IsClubMemberAsync(request.ClubId, userId))
                throw new ApiException("You are already a member of this club.");

            // Check if there's already a pending request
            if (await _clubRepository.HasPendingJoinRequestAsync(request.ClubId, userId))
                throw new ApiException("You already have a pending join request for this club.");

            var joinRequest = new ClubJoinRequest
            {
                UserId = userId,
                ClubId = request.ClubId,
                Status = ClubJoinStatus.Pending,
                Created = DateTime.UtcNow,
                CreatedBy = userId
            };

            await _clubRepository.AddJoinRequestAsync(joinRequest);

            return new Response<int>(joinRequest.Id, "Join request sent successfully.");
        }
    }
}
