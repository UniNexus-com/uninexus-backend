using CleanArchitecture.Core.DTOs.Clubs;
using CleanArchitecture.Core.Interfaces;
using CleanArchitecture.Core.Wrappers;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CleanArchitecture.Core.Features.Clubs.Queries.GetManagedClubs
{
    public class GetManagedClubsQuery : IRequest<Response<IEnumerable<ManagedClubDto>>>
    {
    }

    public class GetManagedClubsQueryHandler : IRequestHandler<GetManagedClubsQuery, Response<IEnumerable<ManagedClubDto>>>
    {
        private readonly IClubRepositoryAsync _clubRepository;
        private readonly IAuthenticatedUserService _authenticatedUserService;

        public GetManagedClubsQueryHandler(IClubRepositoryAsync clubRepository, IAuthenticatedUserService authenticatedUserService)
        {
            _clubRepository = clubRepository;
            _authenticatedUserService = authenticatedUserService;
        }

        public async Task<Response<IEnumerable<ManagedClubDto>>> Handle(GetManagedClubsQuery request, CancellationToken cancellationToken)
        {
            var userId = _authenticatedUserService.UserId;
            var clubs = await _clubRepository.GetManagedClubsAsync(userId);
            return new Response<IEnumerable<ManagedClubDto>>(clubs);
        }
    }
}
