using CleanArchitecture.Core.Enums;
using CleanArchitecture.Core.Exceptions;
using CleanArchitecture.Core.Interfaces;
using CleanArchitecture.Core.Wrappers;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CleanArchitecture.Core.Features.Clubs.Commands.DeclineJoinRequest
{
    public class DeclineJoinRequestCommand : IRequest<Response<int>>
    {
        public int Id { get; set; }
    }

    public class DeclineJoinRequestCommandHandler : IRequestHandler<DeclineJoinRequestCommand, Response<int>>
    {
        private readonly IClubRepositoryAsync _clubRepository;
        private readonly IAuthenticatedUserService _authenticatedUserService;

        public DeclineJoinRequestCommandHandler(IClubRepositoryAsync clubRepository, IAuthenticatedUserService authenticatedUserService)
        {
            _clubRepository = clubRepository;
            _authenticatedUserService = authenticatedUserService;
        }

        public async Task<Response<int>> Handle(DeclineJoinRequestCommand request, CancellationToken cancellationToken)
        {
            var joinRequest = await _clubRepository.GetJoinRequestByIdAsync(request.Id);
            if (joinRequest == null) throw new ApiException("Join request not found.");

            if (!await _clubRepository.HasPrivilegeInClubAsync(joinRequest.ClubId, _authenticatedUserService.UserId, "Manage Members"))
                throw new ApiException("You do not have permission to manage members in this club.");

            if (joinRequest.Status != ClubJoinStatus.Pending) throw new ApiException("Request already processed.");

            joinRequest.Status = ClubJoinStatus.Rejected;
            joinRequest.ProcessedBy = _authenticatedUserService.UserId;
            joinRequest.ProcessedDate = DateTime.UtcNow;
            
            await _clubRepository.UpdateJoinRequestAsync(joinRequest);

            return new Response<int>(joinRequest.Id, "Request declined.");
        }
    }
}
