using CleanArchitecture.Core.DTOs.Clubs;
using CleanArchitecture.Core.Entities;
using CleanArchitecture.Core.Interfaces;
using CleanArchitecture.Core.Wrappers;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CleanArchitecture.Core.Features.Clubs.Queries.GetAllClubs
{
    public class GetAllClubsQuery : IRequest<Response<IEnumerable<ClubViewModel>>>
    {
    }

    public class GetAllClubsQueryHandler : IRequestHandler<GetAllClubsQuery, Response<IEnumerable<ClubViewModel>>>
    {
        private readonly IGenericRepositoryAsync<Club> _clubRepository;
        private readonly IApplicationDbContext _context;
        private readonly IAccountService _accountService;
        private readonly IAuthenticatedUserService _authenticatedUserService;

        public GetAllClubsQueryHandler(
            IGenericRepositoryAsync<Club> clubRepository, 
            IApplicationDbContext context, 
            IAccountService accountService,
            IAuthenticatedUserService authenticatedUserService)
        {
            _clubRepository = clubRepository;
            _context = context;
            _accountService = accountService;
            _authenticatedUserService = authenticatedUserService;
        }

        public async Task<Response<IEnumerable<ClubViewModel>>> Handle(GetAllClubsQuery request, CancellationToken cancellationToken)
        {
            var userId = _authenticatedUserService.UserId;

            // Fetch all clubs with their president info and member counts.
            // Using subqueries in Select is more reliable for LEFT JOIN scenarios in EF Core.
            var clubsData = await _context.Clubs
                .AsNoTracking()
                .Select(club => new
                {
                    Club = club,
                    PresidentUserId = _context.UserClubs
                        .Where(uc => uc.ClubId == club.Id && uc.ClubRoleId == 1 && uc.IsActive)
                        .Select(uc => uc.UserId)
                        .FirstOrDefault(),
                    MemberCount = _context.UserClubs.Count(x => x.ClubId == club.Id && x.IsActive)
                })
                .ToListAsync(cancellationToken);

            // Fetch names for all identified presidents
            var presidentUserIds = clubsData.Where(x => x.PresidentUserId != null).Select(x => x.PresidentUserId).Distinct().ToList();
            var presidentNames = await _accountService.GetUserNamesAsync(presidentUserIds);

            // Fetch current user's status for these clubs
            var joinedClubIds = new HashSet<int>();
            var pendingClubIds = new HashSet<int>();

            if (!string.IsNullOrEmpty(userId))
            {
                var joined = await _context.UserClubs
                    .Where(uc => uc.UserId == userId && uc.IsActive)
                    .Select(uc => uc.ClubId)
                    .ToListAsync(cancellationToken);
                joinedClubIds = new HashSet<int>(joined);

                var pending = await _context.ClubJoinRequests
                    .Where(jr => jr.UserId == userId && jr.Status == Core.Enums.ClubJoinStatus.Pending)
                    .Select(jr => jr.ClubId)
                    .ToListAsync(cancellationToken);
                pendingClubIds = new HashSet<int>(pending);
            }

            var clubViewModels = clubsData.Select(x => new ClubViewModel
            {
                Id = x.Club.Id,
                Name = x.Club.Name,
                Description = x.Club.Description,
                Category = x.Club.Category ?? "OTHER",
                LogoUrl = x.Club.LogoUrl,
                IsActive = x.Club.IsActive,
                Status = x.Club.Status ?? (x.Club.IsActive ? "ACTIVE" : "CLOSED"),
                TotalBudget = x.Club.TotalBudget,
                MemberCount = x.MemberCount,
                Created = x.Club.Created,
                CreatedBy = x.Club.CreatedBy,
                LeaderName = x.PresidentUserId != null && presidentNames.TryGetValue(x.PresidentUserId, out var name) ? name : "Unknown Leader",
                IsJoined = joinedClubIds.Contains(x.Club.Id),
                IsPending = pendingClubIds.Contains(x.Club.Id)
            }).ToList();

            return new Response<IEnumerable<ClubViewModel>>(clubViewModels);
        }
    }
}
