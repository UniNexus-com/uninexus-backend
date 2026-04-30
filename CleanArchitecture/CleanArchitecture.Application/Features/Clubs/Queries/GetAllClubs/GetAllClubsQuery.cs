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

            // 1. Get the system role ID for 'President' dynamically to avoid hardcoded values
            var presidentRole = await _context.ClubRoles
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Name == "President" && r.IsSystemRole, cancellationToken);
            
            var presidentRoleId = presidentRole?.Id ?? 1;

            // 2. Fetch clubs with counts and president IDs in a single query
            // Projecting to an anonymous type first allows EF to optimize the SQL
            var clubsData = await _context.Clubs
                .AsNoTracking()
                .Select(club => new
                {
                    Club = club,
                    PresidentUserId = _context.UserClubs
                        .Where(uc => uc.ClubId == club.Id && uc.ClubRoleId == presidentRoleId && uc.IsActive)
                        .Select(uc => uc.UserId)
                        .FirstOrDefault(),
                    MemberCount = _context.UserClubs.Count(uc => uc.ClubId == club.Id && uc.IsActive),
                    IsJoined = !string.IsNullOrEmpty(userId) && _context.UserClubs.Any(uc => uc.UserId == userId && uc.ClubId == club.Id && uc.IsActive),
                    IsPending = !string.IsNullOrEmpty(userId) && _context.ClubJoinRequests.Any(jr => jr.UserId == userId && jr.ClubId == club.Id && jr.Status == Core.Enums.ClubJoinStatus.Pending)
                })
                .ToListAsync(cancellationToken);

            // 3. Batch fetch president names in a single roundtrip
            var presidentUserIds = clubsData
                .Where(x => x.PresidentUserId != null)
                .Select(x => x.PresidentUserId)
                .Distinct()
                .ToList();

            var presidentNames = presidentUserIds.Any() 
                ? await _accountService.GetUserNamesAsync(presidentUserIds)
                : new Dictionary<string, string>();

            // 4. Map to ViewModel
            var clubViewModels = clubsData.Select(x => new ClubViewModel
            {
                Id          = x.Club.Id,
                Name        = x.Club.Name,
                Description = x.Club.Description,
                Category    = x.Club.Category ?? "OTHER",
                LogoUrl     = x.Club.LogoUrl,
                IsActive    = x.Club.IsActive,
                Status      = x.Club.Status ?? (x.Club.IsActive ? "ACTIVE" : "CLOSED"),
                TotalBudget = x.Club.TotalBudget,
                MemberCount = x.MemberCount,
                Created     = x.Club.Created,
                CreatedBy   = x.Club.CreatedBy,
                LeaderName  = x.PresidentUserId != null && presidentNames.TryGetValue(x.PresidentUserId, out var name) ? name : "Unknown Leader",
                IsJoined    = x.IsJoined,
                IsPending   = x.IsPending
            }).ToList();

            return new Response<IEnumerable<ClubViewModel>>(clubViewModels);
        }
    }
}
