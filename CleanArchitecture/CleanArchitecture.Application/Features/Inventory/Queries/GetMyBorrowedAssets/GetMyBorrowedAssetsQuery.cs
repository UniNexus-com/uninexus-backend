using CleanArchitecture.Core.DTOs.Inventory;
using CleanArchitecture.Core.Features.Inventory.Queries.GetInventory;
using CleanArchitecture.Core.Interfaces;
using CleanArchitecture.Core.Wrappers;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CleanArchitecture.Core.Features.Inventory.Queries.GetMyBorrowedAssets
{
    public class GetMyBorrowedAssetsQuery : IRequest<Response<IReadOnlyList<AssetViewModel>>>
    {
    }

    public class GetMyBorrowedAssetsQueryHandler : IRequestHandler<GetMyBorrowedAssetsQuery, Response<IReadOnlyList<AssetViewModel>>>
    {
        private readonly IAssetRepositoryAsync _assetRepository;
        private readonly IAuthenticatedUserService _authenticatedUserService;

        public GetMyBorrowedAssetsQueryHandler(IAssetRepositoryAsync assetRepository, IAuthenticatedUserService authenticatedUserService)
        {
            _assetRepository = assetRepository;
            _authenticatedUserService = authenticatedUserService;
        }

        public async Task<Response<IReadOnlyList<AssetViewModel>>> Handle(GetMyBorrowedAssetsQuery request, CancellationToken cancellationToken)
        {
            var userId = _authenticatedUserService.UserId;
            var data = await _assetRepository.GetBorrowedAssetsByUserAsync(userId);
            return new Response<IReadOnlyList<AssetViewModel>>(data);
        }
    }
}
