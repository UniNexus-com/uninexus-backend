using AutoMapper;
using CleanArchitecture.Core.DTOs.Roles;
using CleanArchitecture.Core.Entities;
using CleanArchitecture.Core.Features.Roles.Commands.CreateClubRole;
using System.Linq;

namespace CleanArchitecture.Core.Mappings
{
    public class ClubRoleProfile : Profile
    {
        public ClubRoleProfile()
        {
            CreateMap<ClubRole, ClubRoleViewModel>()
                .ForMember(dest => dest.PrivilegeIds, opt => opt.MapFrom(src => src.RolePrivileges.Select(rp => rp.PrivilegeId)));
            CreateMap<CreateClubRoleCommand, ClubRole>()
                .ForMember(dest => dest.RolePrivileges, opt => opt.Ignore());
            CreateMap<ClubPrivilege, ClubPrivilegeViewModel>();
        }
    }
}
