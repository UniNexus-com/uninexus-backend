using CleanArchitecture.Core.DTOs.Clubs;
using CleanArchitecture.Core.Interfaces;
using CleanArchitecture.Core.Wrappers;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CleanArchitecture.Core.Features.Clubs.Queries.GetClubJoinRequests
{
    public class GetClubJoinRequestsQuery : IRequest<Response<IEnumerable<ClubJoinRequestDto>>>
    {
        public int ClubId { get; set; }
    }

    public class GetClubJoinRequestsQueryHandler : IRequestHandler<GetClubJoinRequestsQuery, Response<IEnumerable<ClubJoinRequestDto>>>
    {
        private readonly IClubRepositoryAsync _clubRepository;

        public GetClubJoinRequestsQueryHandler(IClubRepositoryAsync clubRepository)
        {
            _clubRepository = clubRepository;
        }

        public async Task<Response<IEnumerable<ClubJoinRequestDto>>> Handle(GetClubJoinRequestsQuery request, CancellationToken cancellationToken)
        {
            var joinRequests = await _clubRepository.GetClubJoinRequestsAsync(request.ClubId);
            return new Response<IEnumerable<ClubJoinRequestDto>>(joinRequests);
        }
    }
}
