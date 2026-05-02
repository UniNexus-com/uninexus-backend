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
        public string SortBy { get; set; } = "Created";
        public bool IsDescending { get; set; } = true;
    }

    public class GetPagedBudgetRequestsQueryHandler : IRequestHandler<GetPagedBudgetRequestsQuery, PagedResponse<BudgetRequestViewModel>>
    {
        private readonly IApplicationDbContext _context;

        public GetPagedBudgetRequestsQueryHandler(IApplicationDbContext context)
        {
            _context = context;
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

            // Dynamic Sorting
            query = request.SortBy?.ToLower() switch
            {
                "title"    => request.IsDescending ? query.OrderByDescending(r => r.Title) : query.OrderBy(r => r.Title),
                "clubname" => request.IsDescending ? query.OrderByDescending(r => r.Club.Name) : query.OrderBy(r => r.Club.Name),
                "category" => request.IsDescending ? query.OrderByDescending(r => r.Category) : query.OrderBy(r => r.Category),
                "amount"   => request.IsDescending ? query.OrderByDescending(r => r.Amount) : query.OrderBy(r => r.Amount),
                _          => request.IsDescending ? query.OrderByDescending(r => r.Created) : query.OrderBy(r => r.Created)
            };

            // Optimization: Single query projection to fetch everything (including President name) in ONE roundtrip
            var viewModels = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(r => new BudgetRequestViewModel
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
                    ClubName = r.Club != null ? r.Club.Name : $"Club #{r.ClubId}",
                    // Subquery for President name is translated to a join/subquery by EF Core
                    CreatedByName = _context.UserClubs
                        .Where(uc => uc.ClubId == r.ClubId && uc.Role.Name == "President" && uc.IsActive)
                        .Join(_context.Set<ApplicationUser>(), 
                              uc => uc.UserId, 
                              u => u.Id, 
                              (uc, u) => u.FullName)
                        .FirstOrDefault() ?? "Unknown Leader"
                })
                .ToListAsync(cancellationToken);

            return new PagedResponse<BudgetRequestViewModel>(viewModels, request.PageNumber, request.PageSize, totalCount);
        }
    }
}
