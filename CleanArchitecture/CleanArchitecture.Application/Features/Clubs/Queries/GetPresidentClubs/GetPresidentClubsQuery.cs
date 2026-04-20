using CleanArchitecture.Core.Entities;
using CleanArchitecture.Core.Interfaces;
using CleanArchitecture.Core.Wrappers;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CleanArchitecture.Core.Features.Clubs.Queries.GetPresidentClubs
{
    public class GetPresidentClubsQuery : IRequest<Response<IEnumerable<Club>>>
    {
    }

    public class GetPresidentClubsQueryHandler : IRequestHandler<GetPresidentClubsQuery, Response<IEnumerable<Club>>>
    {
        private readonly IClubRepositoryAsync _clubRepository;
        private readonly IAuthenticatedUserService _authenticatedUserService;

        public GetPresidentClubsQueryHandler(IClubRepositoryAsync clubRepository, IAuthenticatedUserService authenticatedUserService)
        {
            _clubRepository = clubRepository;
            _authenticatedUserService = authenticatedUserService;
        }

        public async Task<Response<IEnumerable<Club>>> Handle(GetPresidentClubsQuery request, CancellationToken cancellationToken)
        {
            var userId = _authenticatedUserService.UserId;
            var clubs = await _clubRepository.GetPresidentClubsAsync(userId);
            return new Response<IEnumerable<Club>>(clubs);
        }
    }
}
