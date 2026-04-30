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
            var (members, totalCount) = await _clubRepository.GetClubMembersPagedAsync(request.ClubId, request.PageNumber, request.PageSize, request.SearchValue);
            System.Console.WriteLine($"[DIAGNOSTIC] GetClubMembersQueryHandler - ClubId: {request.ClubId}, Search: {request.SearchValue}, Page: {request.PageNumber}, Count: {members.Count}, Total: {totalCount}");
            return new PagedResponse<ClubMemberDto>(members.ToList(), request.PageNumber, request.PageSize, totalCount);
        }
    }
}
