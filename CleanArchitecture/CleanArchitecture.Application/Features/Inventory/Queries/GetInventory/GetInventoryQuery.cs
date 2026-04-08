using AutoMapper;
using CleanArchitecture.Core.DTOs.Inventory;
using CleanArchitecture.Core.Entities;
using CleanArchitecture.Core.Interfaces;
using CleanArchitecture.Core.Wrappers;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CleanArchitecture.Core.Features.Inventory.Queries.GetInventory
{
    public class GetInventoryQuery : IRequest<Response<IEnumerable<AssetViewModel>>>
    {
        public int? ClubId { get; set; }
    }

    public class GetInventoryQueryHandler : IRequestHandler<GetInventoryQuery, Response<IEnumerable<AssetViewModel>>>
    {
        private readonly IGenericRepositoryAsync<Asset> _assetRepository;
        private readonly IMapper _mapper;

        public GetInventoryQueryHandler(IGenericRepositoryAsync<Asset> assetRepository, IMapper mapper)
        {
            _assetRepository = assetRepository;
            _mapper = mapper;
        }

        public async Task<Response<IEnumerable<AssetViewModel>>> Handle(GetInventoryQuery request, CancellationToken cancellationToken)
        {
            var allAssets = await _assetRepository.GetAllAsync();
            var filtered = request.ClubId.HasValue
                ? allAssets.Where(a => a.ClubId == request.ClubId)
                : allAssets;
            var viewModels = _mapper.Map<IEnumerable<AssetViewModel>>(filtered);
            return new Response<IEnumerable<AssetViewModel>>(viewModels);
        }
    }
}
