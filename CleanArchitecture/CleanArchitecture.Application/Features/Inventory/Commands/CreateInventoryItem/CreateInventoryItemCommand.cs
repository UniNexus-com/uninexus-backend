using AutoMapper;
using CleanArchitecture.Core.Entities;
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

        public CreateInventoryItemCommandHandler(IGenericRepositoryAsync<Asset> assetRepository, IMapper mapper)
        {
            _assetRepository = assetRepository;
            _mapper = mapper;
        }

        public async Task<Response<int>> Handle(CreateInventoryItemCommand request, CancellationToken cancellationToken)
        {
            var asset = _mapper.Map<Asset>(request);
            asset.Status = "AVAILABLE";
            await _assetRepository.AddAsync(asset);
            return new Response<int>(asset.Id);
        }
    }
}
