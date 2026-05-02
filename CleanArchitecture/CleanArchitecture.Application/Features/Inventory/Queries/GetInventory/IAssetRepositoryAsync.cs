using CleanArchitecture.Core.DTOs.Inventory;
using CleanArchitecture.Core.Entities;
using CleanArchitecture.Core.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CleanArchitecture.Core.Features.Inventory.Queries.GetInventory
{
    public interface IAssetRepositoryAsync : IGenericRepositoryAsync<Asset>
    {
        Task<(IReadOnlyList<AssetViewModel> Data, int TotalCount)> GetAssetsPagedAsync(
            int? clubId,
            int pageNumber,
            int pageSize,
            string searchValue,
            string sortColumn,
            string sortDirection,
            List<string> categoryFilters,
            List<string> conditionFilters,
            List<string> statusFilters);
    }
}