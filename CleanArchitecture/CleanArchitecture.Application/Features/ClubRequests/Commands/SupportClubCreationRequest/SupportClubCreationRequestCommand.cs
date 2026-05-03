using CleanArchitecture.Core.Entities;
using CleanArchitecture.Core.Exceptions;
using CleanArchitecture.Core.Interfaces;
using CleanArchitecture.Core.Wrappers;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CleanArchitecture.Core.Features.ClubRequests.Commands.SupportClubCreationRequest
{
    public class SupportClubCreationRequestCommand : IRequest<Response<string>>
    {
        public int RequestId { get; set; }
    }

    public class SupportClubCreationRequestCommandHandler : IRequestHandler<SupportClubCreationRequestCommand, Response<string>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IAuthenticatedUserService _authenticatedUserService;

        public SupportClubCreationRequestCommandHandler(
            IApplicationDbContext context,
            IAuthenticatedUserService authenticatedUserService)
        {
            _context = context;
            _authenticatedUserService = authenticatedUserService;
        }

        public async Task<Response<string>> Handle(SupportClubCreationRequestCommand request, CancellationToken cancellationToken)
        {
            var currentUserId = _authenticatedUserService.UserId;

            var creationRequest = await _context.ClubCreationRequests
                .FirstOrDefaultAsync(r => r.Id == request.RequestId, cancellationToken);

            if (creationRequest == null)
                throw new ApiException("Club creation request not found.");

            if (creationRequest.Status != "GATHERING_SUPPORTERS")
                throw new ApiException("This request is no longer accepting supporters.");

            if (creationRequest.RequesterUserId == currentUserId)
                throw new ApiException("You cannot support your own club creation request.");

            var alreadySupported = await _context.ClubCreationRequestSupporters
                .AnyAsync(s => s.ClubCreationRequestId == request.RequestId && s.UserId == currentUserId,
                          cancellationToken);

            if (alreadySupported)
                throw new ApiException("You have already supported this request.");

            var supporter = new ClubCreationRequestSupporter
            {
                ClubCreationRequestId = request.RequestId,
                UserId = currentUserId,
                SupportedAt = DateTime.UtcNow
            };

            await _context.ClubCreationRequestSupporters.AddAsync(supporter, cancellationToken);

            creationRequest.SupporterCount += 1;

            if (creationRequest.SupporterCount >= 50)
                creationRequest.Status = "PENDING";

            _context.ClubCreationRequests.Update(creationRequest);
            await _context.SaveChangesAsync(cancellationToken);

            var message = creationRequest.Status == "PENDING"
                ? "Support recorded! This request has reached 50 supporters and will now be reviewed by the administration."
                : "Your support has been recorded successfully.";

            return new Response<string>("ok", message);
        }
    }
}
