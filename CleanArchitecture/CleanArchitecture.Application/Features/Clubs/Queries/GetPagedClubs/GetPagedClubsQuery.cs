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

namespace CleanArchitecture.Core.Features.Clubs.Queries.GetPagedClubs
{
    public class GetPagedClubsQuery : IRequest<PagedResponse<ClubViewModel>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string SearchValue { get; set; }
        public string Status { get; set; }
        public string Category { get; set; }
        public string SortBy { get; set; } = "Name";
        public bool IsDescending { get; set; } = false;
    }

    public class GetPagedClubsQueryHandler : IRequestHandler<GetPagedClubsQuery, PagedResponse<ClubViewModel>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IAccountService _accountService;
        private readonly IAuthenticatedUserService _authenticatedUserService;

        public GetPagedClubsQueryHandler(
            IApplicationDbContext context,
            IAccountService accountService,
            IAuthenticatedUserService authenticatedUserService)
        {
            _context = context;
            _accountService = accountService;
            _authenticatedUserService = authenticatedUserService;
        }

        public async Task<PagedResponse<ClubViewModel>> Handle(GetPagedClubsQuery request, CancellationToken cancellationToken)
        {
            var userId = _authenticatedUserService.UserId;

            var presidentRole = await _context.ClubRoles
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Name == "President" && r.IsSystemRole, cancellationToken);
            var presidentRoleId = presidentRole?.Id ?? 1;

            var query = _context.Clubs.AsNoTracking().AsQueryable();

            if (!string.IsNullOrEmpty(request.Status))
                query = query.Where(c => c.Status == request.Status);

            if (!string.IsNullOrEmpty(request.Category))
                query = query.Where(c => c.Category == request.Category);

            if (!string.IsNullOrEmpty(request.SearchValue))
            {
                var search = $"%{request.SearchValue}%";
                // Search by name; leader name search is done after president name batch-fetch below
                query = query.Where(c => EF.Functions.ILike(c.Name, search));
            }

            var totalCount = await query.CountAsync(cancellationToken);

            query = request.SortBy?.ToLower() switch
            {
                "status"      => request.IsDescending ? query.OrderByDescending(c => c.Status) : query.OrderBy(c => c.Status),
                "category"    => request.IsDescending ? query.OrderByDescending(c => c.Category) : query.OrderBy(c => c.Category),
                "membercount" => request.IsDescending
                    ? query.OrderByDescending(c => _context.UserClubs.Count(uc => uc.ClubId == c.Id && uc.IsActive))
                    : query.OrderBy(c => _context.UserClubs.Count(uc => uc.ClubId == c.Id && uc.IsActive)),
                "created"     => request.IsDescending ? query.OrderByDescending(c => c.Created) : query.OrderBy(c => c.Created),
                _             => request.IsDescending ? query.OrderByDescending(c => c.Name) : query.OrderBy(c => c.Name)
            };

            var clubsData = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
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

            var presidentUserIds = clubsData
                .Where(x => x.PresidentUserId != null)
                .Select(x => x.PresidentUserId)
                .Distinct()
                .ToList();

            var presidentNames = presidentUserIds.Any()
                ? await _accountService.GetUserNamesAsync(presidentUserIds)
                : new Dictionary<string, string>();

            var data = clubsData.Select(x => new ClubViewModel
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

            return new PagedResponse<ClubViewModel>(data, request.PageNumber, request.PageSize, totalCount);
        }
    }
}
