using CleanArchitecture.Core.Entities;
using CleanArchitecture.Core.Exceptions;
using CleanArchitecture.Core.Interfaces;
using CleanArchitecture.Core.Wrappers;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CleanArchitecture.Core.Features.ClubRequests.Commands.CreateClubCreationRequest
{
    public class CreateClubCreationRequestCommand : IRequest<Response<int>>
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string AdvisorName { get; set; }
    }

    public class CreateClubCreationRequestCommandHandler : IRequestHandler<CreateClubCreationRequestCommand, Response<int>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IAuthenticatedUserService _authenticatedUserService;

        public CreateClubCreationRequestCommandHandler(
            IApplicationDbContext context,
            IAuthenticatedUserService authenticatedUserService)
        {
            _context = context;
            _authenticatedUserService = authenticatedUserService;
        }

        public async Task<Response<int>> Handle(CreateClubCreationRequestCommand request, CancellationToken cancellationToken)
        {
            var currentUserId = _authenticatedUserService.UserId;

            var hasActiveRequest = await _context.ClubCreationRequests
                .AnyAsync(r => r.RequesterUserId == currentUserId &&
                               (r.Status == "GATHERING_SUPPORTERS" || r.Status == "PENDING"),
                          cancellationToken);

            if (hasActiveRequest)
                throw new ApiException("You already have an active club creation request. Please wait for it to be processed.");

            var entity = new ClubCreationRequest
            {
                Name = request.Name,
                Description = request.Description,
                Category = request.Category,
                AdvisorName = request.AdvisorName,
                RequesterUserId = currentUserId,
                Status = "GATHERING_SUPPORTERS"
            };

            await _context.ClubCreationRequests.AddAsync(entity, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            var creatorSupport = new ClubCreationRequestSupporter
            {
                ClubCreationRequestId = entity.Id,
                UserId = currentUserId,
                SupportedAt = System.DateTime.UtcNow
            };
            entity.SupporterCount = 1;
            await _context.ClubCreationRequestSupporters.AddAsync(creatorSupport, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return new Response<int>(entity.Id, "Your club creation request has been submitted. It needs 2 supporters before it can be reviewed by the administration.");
        }
    }
}
