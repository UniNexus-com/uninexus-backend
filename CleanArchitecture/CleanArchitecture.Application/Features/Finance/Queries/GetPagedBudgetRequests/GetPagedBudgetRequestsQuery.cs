using CleanArchitecture.Core.DTOs.Finance;
using CleanArchitecture.Core.Entities;
using CleanArchitecture.Core.Interfaces;
using CleanArchitecture.Core.Wrappers;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CleanArchitecture.Core.Features.Finance.Queries.GetPagedBudgetRequests
{
    public class GetPagedBudgetRequestsQuery : IRequest<PagedResponse<BudgetRequestViewModel>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string SearchValue { get; set; }
        public string Status { get; set; } = "PENDING";
        public List<int> ClubIds { get; set; }
        public List<string> Categories { get; set; }
    }

    public class GetPagedBudgetRequestsQueryHandler : IRequestHandler<GetPagedBudgetRequestsQuery, PagedResponse<BudgetRequestViewModel>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IAccountService _accountService;

        public GetPagedBudgetRequestsQueryHandler(IApplicationDbContext context, IAccountService accountService)
        {
            _context = context;
            _accountService = accountService;
        }

        public async Task<PagedResponse<BudgetRequestViewModel>> Handle(GetPagedBudgetRequestsQuery request, CancellationToken cancellationToken)
        {
            var query = _context.BudgetRequests
                .Include(r => r.Club)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrEmpty(request.Status))
                query = query.Where(r => r.Status == request.Status);

            if (request.ClubIds != null && request.ClubIds.Any())
                query = query.Where(r => r.ClubId.HasValue && request.ClubIds.Contains(r.ClubId.Value));

            if (request.Categories != null && request.Categories.Any())
                query = query.Where(r => request.Categories.Contains(r.Category));

            if (!string.IsNullOrEmpty(request.SearchValue))
            {
                var search = request.SearchValue.ToLower();
                query = query.Where(r => 
                    r.Title.ToLower().Contains(search) || 
                    (r.Club != null && r.Club.Name.ToLower().Contains(search)) ||
                    r.Description.ToLower().Contains(search));
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var pagedData = await query
                .OrderByDescending(r => r.Created)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            // Fetch President Role ID
            var presidentRole = await _context.ClubRoles
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Name == "President" && r.IsSystemRole, cancellationToken);
            var presidentRoleId = presidentRole?.Id ?? 1;

            // Batch fetch presidents for the clubs in paged result
            var clubIds = pagedData.Select(r => r.ClubId).Where(id => id.HasValue).Select(id => id.Value).Distinct().ToList();
            
            var presidents = await _context.UserClubs
                .Where(uc => clubIds.Contains(uc.ClubId) && uc.ClubRoleId == presidentRoleId && uc.IsActive)
                .ToListAsync(cancellationToken);

            var presidentUserIds = presidents.Select(p => p.UserId).Distinct().ToList();
            var presidentNames = presidentUserIds.Any() 
                ? await _accountService.GetUserNamesAsync(presidentUserIds)
                : new Dictionary<string, string>();

            var clubPresidentDict = presidents
                .GroupBy(p => p.ClubId)
                .ToDictionary(
                    g => g.Key, 
                    g => presidentNames.TryGetValue(g.First().UserId, out var name) ? name : "Unknown Leader"
                );

            var viewModels = pagedData.Select(r => new BudgetRequestViewModel
            {
                Id = r.Id,
                Title = r.Title,
                Category = r.Category,
                Description = r.Description,
                Amount = r.Amount,
                Status = r.Status,
                Created = r.Created,
                CreatedBy = r.CreatedBy,
                ClubId = r.ClubId,
                ClubName = r.Club?.Name ?? $"Club #{r.ClubId}",
                CreatedByName = r.ClubId.HasValue && clubPresidentDict.TryGetValue(r.ClubId.Value, out var leader) ? leader : "Unknown Leader"
            }).ToList();

            return new PagedResponse<BudgetRequestViewModel>(viewModels, request.PageNumber, request.PageSize, totalCount);
        }
    }
}
