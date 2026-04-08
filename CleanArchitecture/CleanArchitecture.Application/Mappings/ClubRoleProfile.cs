using AutoMapper;
using CleanArchitecture.Core.DTOs.Roles;
using CleanArchitecture.Core.Entities;
using CleanArchitecture.Core.Features.Roles.Commands.CreateClubRole;

namespace CleanArchitecture.Core.Mappings
{
    public class ClubRoleProfile : Profile
    {
        public ClubRoleProfile()
        {
            CreateMap<ClubRole, ClubRoleViewModel>();
            CreateMap<CreateClubRoleCommand, ClubRole>();
        }
    }
}
