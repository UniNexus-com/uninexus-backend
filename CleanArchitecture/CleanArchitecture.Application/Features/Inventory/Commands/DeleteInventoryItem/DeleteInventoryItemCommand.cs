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
        private readonly IAuthenticatedUserService _authenticatedUserService;
        private readonly IClubRepositoryAsync _clubRepository;

        public DeleteInventoryItemCommandHandler(IGenericRepositoryAsync<Asset> assetRepository, IAuthenticatedUserService authenticatedUserService, IClubRepositoryAsync clubRepository)
        {
            _assetRepository = assetRepository;
            _authenticatedUserService = authenticatedUserService;
            _clubRepository = clubRepository;
        }

        public async Task<Response<int>> Handle(DeleteInventoryItemCommand request, CancellationToken cancellationToken)
        {
            var asset = await _assetRepository.GetByIdAsync(request.Id);
            if (asset == null) throw new ApiException("Asset Not Found.");

            if (asset.ClubId.HasValue)
            {
                if (!await _clubRepository.HasPrivilegeInClubAsync(asset.ClubId.Value, _authenticatedUserService.UserId, "Manage Assets"))
                    throw new ApiException("You do not have permission to manage assets in this club.");
            }

            await _assetRepository.DeleteAsync(asset);
            return new Response<int>(asset.Id);
        }
    }
}
