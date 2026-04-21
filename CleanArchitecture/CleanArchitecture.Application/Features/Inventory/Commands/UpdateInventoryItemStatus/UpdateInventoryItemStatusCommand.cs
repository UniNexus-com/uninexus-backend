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
        private readonly IAuthenticatedUserService _authenticatedUserService;
        private readonly IClubRepositoryAsync _clubRepository;

        public UpdateInventoryItemStatusCommandHandler(IGenericRepositoryAsync<Asset> assetRepository, IAuthenticatedUserService authenticatedUserService, IClubRepositoryAsync clubRepository)
        {
            _assetRepository = assetRepository;
            _authenticatedUserService = authenticatedUserService;
            _clubRepository = clubRepository;
        }

        public async Task<Response<int>> Handle(UpdateInventoryItemStatusCommand request, CancellationToken cancellationToken)
        {
            var asset = await _assetRepository.GetByIdAsync(request.Id);
            if (asset == null) throw new ApiException("Asset Not Found.");

            if (asset.ClubId.HasValue)
            {
                if (!await _clubRepository.HasPrivilegeInClubAsync(asset.ClubId.Value, _authenticatedUserService.UserId, "Manage Assets"))
                    throw new ApiException("You do not have permission to manage assets in this club.");
            }

            asset.Status = request.Status;
            await _assetRepository.UpdateAsync(asset);
            return new Response<int>(asset.Id);
        }
    }
}
