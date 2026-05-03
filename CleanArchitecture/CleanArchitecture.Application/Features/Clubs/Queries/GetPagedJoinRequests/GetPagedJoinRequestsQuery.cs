using CleanArchitecture.Core.DTOs.Clubs;
using CleanArchitecture.Core.Interfaces;
using CleanArchitecture.Core.Wrappers;
using MediatR;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CleanArchitecture.Core.Features.Clubs.Queries.GetPagedJoinRequests
{
    public class GetPagedJoinRequestsQuery : IRequest<PagedResponse<ClubJoinRequestDto>>
    {
        public int ClubId { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string SearchValue { get; set; }
        public string SortColumn { get; set; } = "created";
        public string SortDirection { get; set; } = "desc";
    }

    public class GetPagedJoinRequestsQueryHandler : IRequestHandler<GetPagedJoinRequestsQuery, PagedResponse<ClubJoinRequestDto>>
    {
        private readonly IClubRepositoryAsync _clubRepository;

        public GetPagedJoinRequestsQueryHandler(IClubRepositoryAsync clubRepository)
        {
            _clubRepository = clubRepository;
        }

        public async Task<PagedResponse<ClubJoinRequestDto>> Handle(GetPagedJoinRequestsQuery request, CancellationToken cancellationToken)
        {
            var (data, totalCount) = await _clubRepository.GetJoinRequestsPagedAsync(
                request.ClubId,
                request.PageNumber,
                request.PageSize,
                request.SearchValue,
                request.SortColumn,
                request.SortDirection);

            return new PagedResponse<ClubJoinRequestDto>(data.ToList(), request.PageNumber, request.PageSize, totalCount);
        }
    }
}
