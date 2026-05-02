using CleanArchitecture.Core.DTOs.Inventory;
using CleanArchitecture.Core.Entities;
using CleanArchitecture.Core.Features.Inventory.Queries.GetInventory;
using CleanArchitecture.Core.Wrappers;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CleanArchitecture.Core.Features.Inventory.Queries.GetInventory
{
    public class GetInventoryQuery : IRequest<PagedResponse<AssetViewModel>>
    {
        public int? ClubId { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string SearchValue { get; set; }
        public string SortColumn { get; set; } = "Name";
        public string SortDirection { get; set; } = "asc";
        public List<string> CategoryFilters { get; set; }
        public List<string> ConditionFilters { get; set; }
        public List<string> StatusFilters { get; set; }
    }

    public class GetInventoryQueryHandler : IRequestHandler<GetInventoryQuery, PagedResponse<AssetViewModel>>
    {
        private readonly IAssetRepositoryAsync _assetRepository;

        public GetInventoryQueryHandler(IAssetRepositoryAsync assetRepository)
        {
            _assetRepository = assetRepository;
        }

        public async Task<PagedResponse<AssetViewModel>> Handle(GetInventoryQuery request, CancellationToken cancellationToken)
        {
            var (data, totalCount) = await _assetRepository.GetAssetsPagedAsync(
                request.ClubId,
                request.PageNumber,
                request.PageSize,
                request.SearchValue,
                request.SortColumn,
                request.SortDirection,
                request.CategoryFilters,
                request.ConditionFilters,
                request.StatusFilters);

            return new PagedResponse<AssetViewModel>(data.ToList(), request.PageNumber, request.PageSize, totalCount);
        }
    }
}
