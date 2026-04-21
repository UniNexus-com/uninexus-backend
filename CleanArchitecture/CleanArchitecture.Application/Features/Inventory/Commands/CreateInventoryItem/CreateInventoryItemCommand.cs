using AutoMapper;
using CleanArchitecture.Core.Entities;
using CleanArchitecture.Core.Exceptions;
using CleanArchitecture.Core.Interfaces;
using CleanArchitecture.Core.Wrappers;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace CleanArchitecture.Core.Features.Inventory.Commands.CreateInventoryItem
{
    public class CreateInventoryItemCommand : IRequest<Response<int>>
    {
        public string Name { get; set; }
        public string Category { get; set; }
        public string Condition { get; set; }
        public string Location { get; set; }
        public decimal? Value { get; set; }
        public string SerialNo { get; set; }
        public string Description { get; set; }
        public int? ClubId { get; set; }
    }

    public class CreateInventoryItemCommandHandler : IRequestHandler<CreateInventoryItemCommand, Response<int>>
    {
        private readonly IGenericRepositoryAsync<Asset> _assetRepository;
        private readonly IMapper _mapper;
        private readonly IAuthenticatedUserService _authenticatedUserService;
        private readonly IClubRepositoryAsync _clubRepository;

        public CreateInventoryItemCommandHandler(IGenericRepositoryAsync<Asset> assetRepository, IMapper mapper, IAuthenticatedUserService authenticatedUserService, IClubRepositoryAsync clubRepository)
        {
            _assetRepository = assetRepository;
            _mapper = mapper;
            _authenticatedUserService = authenticatedUserService;
            _clubRepository = clubRepository;
        }

        public async Task<Response<int>> Handle(CreateInventoryItemCommand request, CancellationToken cancellationToken)
        {
            if (request.ClubId.HasValue)
            {
                if (!await _clubRepository.HasPrivilegeInClubAsync(request.ClubId.Value, _authenticatedUserService.UserId, "Manage Assets"))
                    throw new ApiException("You do not have permission to manage assets in this club.");
            }

            var asset = _mapper.Map<Asset>(request);
            asset.Status = "AVAILABLE";
            await _assetRepository.AddAsync(asset);
            return new Response<int>(asset.Id);
        }
    }
}
