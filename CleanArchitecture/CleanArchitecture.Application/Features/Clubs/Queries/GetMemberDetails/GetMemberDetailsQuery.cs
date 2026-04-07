using CleanArchitecture.Core.DTOs.Clubs;
using CleanArchitecture.Core.Interfaces;
using CleanArchitecture.Core.Wrappers;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace CleanArchitecture.Core.Features.Clubs.Queries.GetMemberDetails
{
    public class GetMemberDetailsQuery : IRequest<Response<MemberDetailsDto>>
    {
        public int ClubId { get; set; }
        public string UserId { get; set; }
    }

    public class GetMemberDetailsQueryHandler : IRequestHandler<GetMemberDetailsQuery, Response<MemberDetailsDto>>
    {
        private readonly IClubRepositoryAsync _clubRepository;

        public GetMemberDetailsQueryHandler(IClubRepositoryAsync clubRepository)
        {
            _clubRepository = clubRepository;
        }

        public async Task<Response<MemberDetailsDto>> Handle(GetMemberDetailsQuery request, CancellationToken cancellationToken)
        {
            var member = await _clubRepository.GetClubMemberDetailsAsync(request.ClubId, request.UserId);
            return new Response<MemberDetailsDto>(member);
        }
    }
}
