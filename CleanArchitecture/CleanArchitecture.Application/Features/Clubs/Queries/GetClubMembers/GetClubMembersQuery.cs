using CleanArchitecture.Core.DTOs.Clubs;
using CleanArchitecture.Core.Entities;
using CleanArchitecture.Core.Interfaces;
using CleanArchitecture.Core.Wrappers;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CleanArchitecture.Core.Features.Clubs.Queries.GetClubMembers
{
    public class GetClubMembersQuery : IRequest<PagedResponse<ClubMemberDto>>
    {
        public int ClubId { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string SearchValue { get; set; }
        public string SortColumn { get; set; }
        public string SortDirection { get; set; }
        public List<string> RoleFilters { get; set; }
        public List<string> StatusFilters { get; set; }
    }

    public class GetClubMembersQueryHandler : IRequestHandler<GetClubMembersQuery, PagedResponse<ClubMemberDto>>
    {
        private readonly IClubRepositoryAsync _clubRepository;

        public GetClubMembersQueryHandler(IClubRepositoryAsync clubRepository)
        {
            _clubRepository = clubRepository;
        }

        public async Task<PagedResponse<ClubMemberDto>> Handle(GetClubMembersQuery request, CancellationToken cancellationToken)
        {
            var (members, totalCount) = await _clubRepository.GetClubMembersPagedAsync(
                request.ClubId, 
                request.PageNumber, 
                request.PageSize, 
                request.SearchValue,
                request.SortColumn,
                request.SortDirection,
                request.RoleFilters,
                request.StatusFilters);
            
            System.Console.WriteLine($"[DIAGNOSTIC] GetClubMembersQueryHandler - ClubId: {request.ClubId}, Search: '{request.SearchValue}', Sort: {request.SortColumn} {request.SortDirection}, Roles: {string.Join(",", request.RoleFilters ?? new List<string>())}, Statuses: {string.Join(",", request.StatusFilters ?? new List<string>())}");
            return new PagedResponse<ClubMemberDto>(members.ToList(), request.PageNumber, request.PageSize, totalCount);
        }
    }
}
