using AutoMapper;
using CleanArchitecture.Core.DTOs.Clubs;
using CleanArchitecture.Core.Entities;

namespace CleanArchitecture.Core.Mappings
{
    public class GeneralProfile : Profile
    {
        public GeneralProfile()
        {
            CreateMap<ClubCreationRequest, ClubCreationRequestDto>();
        }
    }
}
