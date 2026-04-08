using CleanArchitecture.Core.Entities;
using CleanArchitecture.Core.Exceptions;
using CleanArchitecture.Core.Interfaces;
using CleanArchitecture.Core.Wrappers;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace CleanArchitecture.Core.Features.Inventory.Commands.DeleteInventoryItem
{
    public class DeleteInventoryItemCommand : IRequest<Response<int>>
    {
        public int Id { get; set; }
    }

    public class DeleteInventoryItemCommandHandler : IRequestHandler<DeleteInventoryItemCommand, Response<int>>
    {
        private readonly IGenericRepositoryAsync<Asset> _assetRepository;

        public DeleteInventoryItemCommandHandler(IGenericRepositoryAsync<Asset> assetRepository)
        {
            _assetRepository = assetRepository;
        }

        public async Task<Response<int>> Handle(DeleteInventoryItemCommand request, CancellationToken cancellationToken)
        {
            var asset = await _assetRepository.GetByIdAsync(request.Id);
            if (asset == null) throw new ApiException("Asset Not Found.");
            await _assetRepository.DeleteAsync(asset);
            return new Response<int>(asset.Id);
        }
    }
}
