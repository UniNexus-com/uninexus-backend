using AutoMapper;
using CleanArchitecture.Core.DTOs.Roles;
using CleanArchitecture.Core.Entities;
using CleanArchitecture.Core.Interfaces;
using CleanArchitecture.Core.Wrappers;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CleanArchitecture.Core.Features.Roles.Queries.GetClubPrivileges
{
    public class GetClubPrivilegesQuery : IRequest<Response<IEnumerable<ClubPrivilegeViewModel>>>
    {
    }

    public class GetClubPrivilegesQueryHandler : IRequestHandler<GetClubPrivilegesQuery, Response<IEnumerable<ClubPrivilegeViewModel>>>
    {
        private readonly IApplicationDbContext _dbContext;
        private readonly IMapper _mapper;

        public GetClubPrivilegesQueryHandler(IApplicationDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task<Response<IEnumerable<ClubPrivilegeViewModel>>> Handle(GetClubPrivilegesQuery request, CancellationToken cancellationToken)
        {
            var all = await _dbContext.ClubPrivileges.ToListAsync(cancellationToken);
            return new Response<IEnumerable<ClubPrivilegeViewModel>>(_mapper.Map<IEnumerable<ClubPrivilegeViewModel>>(all));
        }
    }
}
