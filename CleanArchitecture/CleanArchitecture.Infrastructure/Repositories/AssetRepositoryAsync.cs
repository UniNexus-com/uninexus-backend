using CleanArchitecture.Core.DTOs.Inventory;
using CleanArchitecture.Core.Entities;
using CleanArchitecture.Core.Features.Inventory.Queries.GetInventory;
using CleanArchitecture.Core.Interfaces;
using CleanArchitecture.Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CleanArchitecture.Infrastructure.Repository
{
    public class AssetRepositoryAsync : GenericRepositoryAsync<Asset>, IAssetRepositoryAsync
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly DbSet<Asset> _assets;
        private readonly DbSet<AssetLoan> _assetLoans;
        private readonly DbSet<ApplicationUser> _users;

        public AssetRepositoryAsync(ApplicationDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
            _assets = dbContext.Set<Asset>();
            _assetLoans = dbContext.Set<AssetLoan>();
            _users = dbContext.Set<ApplicationUser>();
        }

        public async Task<(IReadOnlyList<AssetViewModel> Data, int TotalCount)> GetAssetsPagedAsync(
            int? clubId,
            int pageNumber,
            int pageSize,
            string searchValue,
            string sortColumn,
            string sortDirection,
            List<string> categoryFilters,
            List<string> conditionFilters,
            List<string> statusFilters)
        {
            // Build base query with filters
            var query = _assets.AsQueryable();

            if (clubId.HasValue)
                query = query.Where(a => a.ClubId == clubId);

            if (!string.IsNullOrEmpty(searchValue))
            {
                var search = searchValue.ToLower();
                query = query.Where(a => 
                    a.Name.ToLower().Contains(search) ||
                    (a.SerialNo != null && a.SerialNo.ToLower().Contains(search)) ||
                    (a.Description != null && a.Description.ToLower().Contains(search)));
            }

            if (categoryFilters != null && categoryFilters.Any())
                query = query.Where(a => categoryFilters.Contains(a.Category));

            if (conditionFilters != null && conditionFilters.Any())
                query = query.Where(a => conditionFilters.Contains(a.Condition));

            if (statusFilters != null && statusFilters.Any())
                query = query.Where(a => statusFilters.Contains(a.Status));

            // Get total count before ordering
            var totalCount = await query.CountAsync();

            // Apply sorting - support all columns
            var isAsc = sortDirection?.ToLower() != "desc";
            var sortCol = sortColumn?.ToLower() ?? "name";
            
            // For ID, we use ID column directly (EF Core handles integer properly)
            // For other columns, use string comparison
            query = sortCol switch
            {
                "id" when isAsc => query.OrderBy(a => a.Id),
                "id" => query.OrderByDescending(a => a.Id),
                "name" when isAsc => query.OrderBy(a => a.Name),
                "name" => query.OrderByDescending(a => a.Name),
                "category" when isAsc => query.OrderBy(a => a.Category),
                "category" => query.OrderByDescending(a => a.Category),
                "condition" when isAsc => query.OrderBy(a => a.Condition),
                "condition" => query.OrderByDescending(a => a.Condition),
                "status" when isAsc => query.OrderBy(a => a.Status),
                "status" => query.OrderByDescending(a => a.Status),
                "value" when isAsc => query.OrderBy(a => a.Value),
                "value" => query.OrderByDescending(a => a.Value),
                "location" when isAsc => query.OrderBy(a => a.Location),
                "location" => query.OrderByDescending(a => a.Location),
                "serialno" when isAsc => query.OrderBy(a => a.SerialNo),
                "serialno" => query.OrderByDescending(a => a.SerialNo),
                _ => isAsc ? query.OrderBy(a => a.Name) : query.OrderByDescending(a => a.Name)
            };

            // Skip and Take for pagination
            var data = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

            // Map to ViewModel
            var result = new List<AssetViewModel>();
            foreach (var asset in data)
            {
                var loan = await _assetLoans
                    .Where(l => l.AssetId == asset.Id && l.Status != "Returned" && l.ReturnedAt == null)
                    .OrderByDescending(l => l.BorrowedAt)
                    .FirstOrDefaultAsync();

                string loanedBy = null;
                string loanedByUserId = null;
                if (loan != null && !string.IsNullOrEmpty(loan.UserId))
                {
                    var user = await _users.FindAsync(loan.UserId);
                    loanedBy = user?.FullName;
                    loanedByUserId = loan.UserId;
                }

                result.Add(new AssetViewModel
                {
                    Id = asset.Id,
                    Name = asset.Name,
                    Category = asset.Category,
                    Condition = asset.Condition,
                    Location = asset.Location,
                    Status = asset.Status,
                    Value = asset.Value,
                    SerialNo = asset.SerialNo,
                    Description = asset.Description,
                    ClubId = asset.ClubId,
                    LoanedBy = loanedBy,
                    LoanedByUserId = loanedByUserId
                });
            }

            return (result, totalCount);
        }
    }
}