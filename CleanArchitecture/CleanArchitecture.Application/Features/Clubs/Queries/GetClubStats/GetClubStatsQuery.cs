using CleanArchitecture.Core.DTOs.Clubs;
using CleanArchitecture.Core.Interfaces;
using CleanArchitecture.Core.Wrappers;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace CleanArchitecture.Core.Features.Clubs.Queries.GetClubStats
{
    public class GetClubStatsQuery : IRequest<Response<ClubStatsDto>>
    {
        public int ClubId { get; set; }
    }

    public class GetClubStatsQueryHandler : IRequestHandler<GetClubStatsQuery, Response<ClubStatsDto>>
    {
        private readonly IClubRepositoryAsync _clubRepository;
        private readonly IAuthenticatedUserService _authenticatedUserService;

        public GetClubStatsQueryHandler(IClubRepositoryAsync clubRepository, IAuthenticatedUserService authenticatedUserService)
        {
            _clubRepository = clubRepository;
            _authenticatedUserService = authenticatedUserService;
        }

        public async Task<Response<ClubStatsDto>> Handle(GetClubStatsQuery request, CancellationToken cancellationToken)
        {
            if (!await _clubRepository.HasPrivilegeInClubAsync(request.ClubId, _authenticatedUserService.UserId, "View Reports"))
                throw new Exceptions.ApiException("You do not have permission to view reports for this club.");

            var stats = await _clubRepository.GetClubStatsAsync(request.ClubId);
            return new Response<ClubStatsDto>(stats);
        }
    }
}
