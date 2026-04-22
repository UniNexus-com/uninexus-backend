using CleanArchitecture.Core.Entities;
using CleanArchitecture.Core.Exceptions;
using CleanArchitecture.Core.Interfaces;
using CleanArchitecture.Core.Wrappers;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CleanArchitecture.Core.Features.Clubs.Commands.ApproveClubCreationRequest
{
    public class ApproveClubCreationRequestCommand : IRequest<Response<int>>
    {
        public int RequestId { get; set; }
    }

    public class ApproveClubCreationRequestCommandHandler : IRequestHandler<ApproveClubCreationRequestCommand, Response<int>>
    {
        private readonly IGenericRepositoryAsync<ClubCreationRequest> _requestRepo;
        private readonly IGenericRepositoryAsync<Club> _clubRepo;
        private readonly IGenericRepositoryAsync<UserClub> _userClubRepo;

        public ApproveClubCreationRequestCommandHandler(
            IGenericRepositoryAsync<ClubCreationRequest> requestRepo,
            IGenericRepositoryAsync<Club> clubRepo,
            IGenericRepositoryAsync<UserClub> userClubRepo)
        {
            _requestRepo = requestRepo;
            _clubRepo = clubRepo;
            _userClubRepo = userClubRepo;
        }

        public async Task<Response<int>> Handle(ApproveClubCreationRequestCommand request, CancellationToken cancellationToken)
        {
            var creationRequest = await _requestRepo.GetByIdAsync(request.RequestId);
            if (creationRequest == null) throw new ApiException("No request found.");
            if (creationRequest.Status != "PENDING") throw new ApiException("This request has already been processed.");

            
            creationRequest.Status = "APPROVED";
            await _requestRepo.UpdateAsync(creationRequest);


            var newClub = new Club
            {
                Name = creationRequest.Name,
                Description = creationRequest.Description,
                IsActive = true,
                Status = "ACTIVE",
                TotalBudget = 0
            };
            await _clubRepo.AddAsync(newClub);

            var userClub = new UserClub
            {
                UserId = creationRequest.RequesterUserId,
                ClubId = newClub.Id,
                JoinDate = DateTime.UtcNow,
                IsActive = true
            };
            await _userClubRepo.AddAsync(userClub);

            return new Response<int>(newClub.Id, $"Club successfully created and {newClub.Name} added to the system.");
        }
    }
}
