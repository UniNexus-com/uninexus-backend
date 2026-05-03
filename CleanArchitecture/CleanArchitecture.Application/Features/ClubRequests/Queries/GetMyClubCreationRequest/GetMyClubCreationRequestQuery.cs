using CleanArchitecture.Core.DTOs.Clubs;
using CleanArchitecture.Core.Entities;
using CleanArchitecture.Core.Interfaces;
using CleanArchitecture.Core.Wrappers;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CleanArchitecture.Core.Features.ClubRequests.Queries.GetMyClubCreationRequest
{
    public class GetMyClubCreationRequestQuery : IRequest<Response<ClubCreationRequestDto>>
    {
    }

    public class GetMyClubCreationRequestQueryHandler : IRequestHandler<GetMyClubCreationRequestQuery, Response<ClubCreationRequestDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IAuthenticatedUserService _authenticatedUserService;

        public GetMyClubCreationRequestQueryHandler(
            IApplicationDbContext context,
            IAuthenticatedUserService authenticatedUserService)
        {
            _context = context;
            _authenticatedUserService = authenticatedUserService;
        }

        public async Task<Response<ClubCreationRequestDto>> Handle(GetMyClubCreationRequestQuery request, CancellationToken cancellationToken)
        {
            var userId = _authenticatedUserService.UserId;

            var myRequest = await _context.ClubCreationRequests
                .Where(r => r.RequesterUserId == userId)
                .OrderByDescending(r => r.Created)
                .FirstOrDefaultAsync(cancellationToken);

            if (myRequest == null)
                return new Response<ClubCreationRequestDto>(null);

            var dto = new ClubCreationRequestDto
            {
                Id = myRequest.Id,
                Name = myRequest.Name,
                Description = myRequest.Description,
                Category = myRequest.Category,
                AdvisorName = myRequest.AdvisorName,
                RequesterUserId = myRequest.RequesterUserId,
                Status = myRequest.Status,
                RejectionReason = myRequest.RejectionReason,
                SupporterCount = myRequest.SupporterCount,
                Created = myRequest.Created
            };

            return new Response<ClubCreationRequestDto>(dto);
        }
    }
}
