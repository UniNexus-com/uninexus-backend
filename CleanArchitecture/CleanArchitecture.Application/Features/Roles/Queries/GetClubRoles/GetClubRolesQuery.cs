using AutoMapper;
using CleanArchitecture.Core.DTOs.Roles;
using CleanArchitecture.Core.Entities;
using CleanArchitecture.Core.Interfaces;
using CleanArchitecture.Core.Wrappers;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CleanArchitecture.Core.Features.Roles.Queries.GetClubRoles
{
    public class GetClubRolesQuery : IRequest<Response<IEnumerable<ClubRoleViewModel>>>
    {
        public int ClubId { get; set; }
    }

    public class GetClubRolesQueryHandler : IRequestHandler<GetClubRolesQuery, Response<IEnumerable<ClubRoleViewModel>>>
    {
        private readonly IGenericRepositoryAsync<ClubRole> _roleRepo;
        private readonly IMapper _mapper;

        public GetClubRolesQueryHandler(IGenericRepositoryAsync<ClubRole> roleRepo, IMapper mapper)
        {
            _roleRepo = roleRepo;
            _mapper = mapper;
        }

        public async Task<Response<IEnumerable<ClubRoleViewModel>>> Handle(GetClubRolesQuery request, CancellationToken cancellationToken)
        {
            var all = await _roleRepo.GetAllAsync();
            var roles = all.Where(r => r.IsSystemRole || r.ClubId == request.ClubId);
            return new Response<IEnumerable<ClubRoleViewModel>>(_mapper.Map<IEnumerable<ClubRoleViewModel>>(roles));
        }
    }
}
