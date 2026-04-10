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

        public GetClubStatsQueryHandler(IClubRepositoryAsync clubRepository)
        {
            _clubRepository = clubRepository;
        }

        public async Task<Response<ClubStatsDto>> Handle(GetClubStatsQuery request, CancellationToken cancellationToken)
        {
            var stats = await _clubRepository.GetClubStatsAsync(request.ClubId);
            return new Response<ClubStatsDto>(stats);
        }
    }
}
