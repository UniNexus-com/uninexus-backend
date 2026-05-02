using CleanArchitecture.Core.DTOs.Clubs;
using CleanArchitecture.Core.Interfaces;
using CleanArchitecture.Core.Wrappers;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace CleanArchitecture.Core.Features.Clubs.Queries.GetClubHistory
{
    public class GetClubHistoryQuery : IRequest<Response<ClubHistoryDto>>
    {
        public int ClubId { get; set; }
    }

    public class GetClubHistoryQueryHandler : IRequestHandler<GetClubHistoryQuery, Response<ClubHistoryDto>>
    {
        private readonly IClubRepositoryAsync _clubRepository;
        private readonly IAuthenticatedUserService _authenticatedUserService;

        public GetClubHistoryQueryHandler(IClubRepositoryAsync clubRepository, IAuthenticatedUserService authenticatedUserService)
        {
            _clubRepository = clubRepository;
            _authenticatedUserService = authenticatedUserService;
        }

        public async Task<Response<ClubHistoryDto>> Handle(GetClubHistoryQuery request, CancellationToken cancellationToken)
        {
            if (!await _clubRepository.HasPrivilegeInClubAsync(request.ClubId, _authenticatedUserService.UserId, "View Reports"))
                throw new Exceptions.ApiException("You do not have permission to view reports for this club.");

            var history = await _clubRepository.GetClubHistoryAsync(request.ClubId);
            return new Response<ClubHistoryDto>(history);
        }
    }
}
