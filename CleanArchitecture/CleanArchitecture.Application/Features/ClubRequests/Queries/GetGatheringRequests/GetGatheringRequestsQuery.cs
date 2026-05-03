using CleanArchitecture.Core.DTOs.Clubs;
using CleanArchitecture.Core.Entities;
using CleanArchitecture.Core.Interfaces;
using CleanArchitecture.Core.Wrappers;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CleanArchitecture.Core.Features.ClubRequests.Queries.GetGatheringRequests
{
    public class GetGatheringRequestsQuery : IRequest<Response<IEnumerable<GatheringClubRequestDto>>>
    {
    }

    public class GetGatheringRequestsQueryHandler : IRequestHandler<GetGatheringRequestsQuery, Response<IEnumerable<GatheringClubRequestDto>>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IAuthenticatedUserService _authenticatedUserService;

        public GetGatheringRequestsQueryHandler(
            IApplicationDbContext context,
            IAuthenticatedUserService authenticatedUserService)
        {
            _context = context;
            _authenticatedUserService = authenticatedUserService;
        }

        public async Task<Response<IEnumerable<GatheringClubRequestDto>>> Handle(GetGatheringRequestsQuery request, CancellationToken cancellationToken)
        {
            var currentUserId = _authenticatedUserService.UserId;

            var gatheringRequests = await _context.ClubCreationRequests
                .Where(r => r.Status == "GATHERING_SUPPORTERS")
                .OrderByDescending(r => r.SupporterCount)
                .ToListAsync(cancellationToken);

            var supportedRequestIds = await _context.ClubCreationRequestSupporters
                .Where(s => s.UserId == currentUserId)
                .Select(s => s.ClubCreationRequestId)
                .ToListAsync(cancellationToken);

            var supportedSet = new HashSet<int>(supportedRequestIds);

            var requesterIds = gatheringRequests.Select(r => r.RequesterUserId).Distinct().ToList();
            var users = await _context.Set<ApplicationUser>()
                .Where(u => requesterIds.Contains(u.Id))
                .Select(u => new { u.Id, u.FullName })
                .ToListAsync(cancellationToken);

            var userNameMap = users.ToDictionary(u => u.Id, u => u.FullName);

            var dtos = gatheringRequests.Select(r => new GatheringClubRequestDto
            {
                Id = r.Id,
                Name = r.Name,
                Description = r.Description,
                Category = r.Category,
                AdvisorName = r.AdvisorName,
                RequesterUserId = r.RequesterUserId,
                RequesterName = userNameMap.TryGetValue(r.RequesterUserId, out var name) ? name : string.Empty,
                SupporterCount = r.SupporterCount,
                MaxSupporters = 50,
                IsSupportedByMe = supportedSet.Contains(r.Id),
                Created = r.Created
            });

            return new Response<IEnumerable<GatheringClubRequestDto>>(dtos);
        }
    }
}
