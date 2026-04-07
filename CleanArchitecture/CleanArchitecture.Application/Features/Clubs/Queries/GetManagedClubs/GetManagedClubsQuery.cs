using CleanArchitecture.Core.Entities;
using CleanArchitecture.Core.Interfaces;
using CleanArchitecture.Core.Wrappers;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CleanArchitecture.Core.Features.Clubs.Queries.GetManagedClubs
{
    public class GetManagedClubsQuery : IRequest<Response<IEnumerable<Club>>>
    {
    }

    public class GetManagedClubsQueryHandler : IRequestHandler<GetManagedClubsQuery, Response<IEnumerable<Club>>>
    {
        private readonly IClubRepositoryAsync _clubRepository;
        private readonly IAuthenticatedUserService _authenticatedUserService;

        public GetManagedClubsQueryHandler(IClubRepositoryAsync clubRepository, IAuthenticatedUserService authenticatedUserService)
        {
            _clubRepository = clubRepository;
            _authenticatedUserService = authenticatedUserService;
        }

        public async Task<Response<IEnumerable<Club>>> Handle(GetManagedClubsQuery request, CancellationToken cancellationToken)
        {
            var userId = _authenticatedUserService.UserId;
            var clubs = await _clubRepository.GetManagedClubsAsync(userId);
            return new Response<IEnumerable<Club>>(clubs);
        }
    }
}
