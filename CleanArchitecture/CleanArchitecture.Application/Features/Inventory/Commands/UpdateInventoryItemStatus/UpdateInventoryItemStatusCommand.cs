using CleanArchitecture.Core.Entities;
using CleanArchitecture.Core.Exceptions;
using CleanArchitecture.Core.Interfaces;
using CleanArchitecture.Core.Wrappers;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace CleanArchitecture.Core.Features.Inventory.Commands.UpdateInventoryItemStatus
{
    public class UpdateInventoryItemStatusCommand : IRequest<Response<int>>
    {
        public int Id { get; set; }
        public string Status { get; set; }
    }

    public class UpdateInventoryItemStatusCommandHandler : IRequestHandler<UpdateInventoryItemStatusCommand, Response<int>>
    {
        private readonly IGenericRepositoryAsync<Asset> _assetRepository;

        public UpdateInventoryItemStatusCommandHandler(IGenericRepositoryAsync<Asset> assetRepository)
        {
            _assetRepository = assetRepository;
        }

        public async Task<Response<int>> Handle(UpdateInventoryItemStatusCommand request, CancellationToken cancellationToken)
        {
            var asset = await _assetRepository.GetByIdAsync(request.Id);
            if (asset == null) throw new ApiException("Asset Not Found.");
            asset.Status = request.Status;
            await _assetRepository.UpdateAsync(asset);
            return new Response<int>(asset.Id);
        }
    }
}
