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

namespace CleanArchitecture.Core.Features.Clubs.Commands.RejectClubCreationRequest
{
    public class RejectClubCreationRequestCommand : IRequest<Response<int>>
    {
        public int RequestId { get; set; }
        public string RejectionReason { get; set; }
    }

    public class RejectClubCreationRequestCommandHandler : IRequestHandler<RejectClubCreationRequestCommand, Response<int>>
    {
        private readonly IGenericRepositoryAsync<ClubCreationRequest> _requestRepo;

        public RejectClubCreationRequestCommandHandler(IGenericRepositoryAsync<ClubCreationRequest> requestRepo)
        {
            _requestRepo = requestRepo;
        }

        public async Task<Response<int>> Handle(RejectClubCreationRequestCommand request, CancellationToken cancellationToken)
        {
            var creationRequest = await _requestRepo.GetByIdAsync(request.RequestId);
            if (creationRequest == null) throw new ApiException("No request found.");
            if (creationRequest.Status != "PENDING") throw new ApiException("This request has already been processed.");

            creationRequest.Status = "REJECTED";
            creationRequest.RejectionReason = request.RejectionReason;

            await _requestRepo.UpdateAsync(creationRequest);

            return new Response<int>(creationRequest.Id, "Club creation request has been rejected.");
        }
    }
}
