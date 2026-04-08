using AutoMapper;
using CleanArchitecture.Core.DTOs.Inventory;
using CleanArchitecture.Core.Entities;
using CleanArchitecture.Core.Features.Inventory.Commands.CreateInventoryItem;

namespace CleanArchitecture.Core.Mappings
{
    public class AssetProfile : Profile
    {
        public AssetProfile()
        {
            CreateMap<Asset, AssetViewModel>();
            CreateMap<CreateInventoryItemCommand, Asset>();
        }
    }
}
