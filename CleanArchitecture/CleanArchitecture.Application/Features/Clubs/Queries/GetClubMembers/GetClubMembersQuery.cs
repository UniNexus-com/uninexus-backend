using CleanArchitecture.Core.DTOs.Clubs;
using CleanArchitecture.Core.Interfaces;
using CleanArchitecture.Core.Wrappers;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CleanArchitecture.Core.Features.Clubs.Queries.GetClubMembers
{
    public class GetClubMembersQuery : IRequest<Response<IEnumerable<ClubMemberDto>>>
    {
        public int ClubId { get; set; }
    }

    public class GetClubMembersQueryHandler : IRequestHandler<GetClubMembersQuery, Response<IEnumerable<ClubMemberDto>>>
    {
        private readonly IClubRepositoryAsync _clubRepository;

        public GetClubMembersQueryHandler(IClubRepositoryAsync clubRepository)
        {
            _clubRepository = clubRepository;
        }

        public async Task<Response<IEnumerable<ClubMemberDto>>> Handle(GetClubMembersQuery request, CancellationToken cancellationToken)
        {
            var members = await _clubRepository.GetClubMembersAsync(request.ClubId);
            return new Response<IEnumerable<ClubMemberDto>>(members);
        }
    }
}
