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

namespace CleanArchitecture.Core.Features.Clubs.Queries.GetAllClubs
{
    public class GetAllClubsQuery : IRequest<Response<IEnumerable<ClubViewModel>>>
    {
    }

    public class GetAllClubsQueryHandler : IRequestHandler<GetAllClubsQuery, Response<IEnumerable<ClubViewModel>>>
    {
        private readonly IGenericRepositoryAsync<Club> _clubRepository;
        private readonly IApplicationDbContext _context;
        private readonly IAccountService _accountService;

        public GetAllClubsQueryHandler(IGenericRepositoryAsync<Club> clubRepository, IApplicationDbContext context, IAccountService accountService)
        {
            _clubRepository = clubRepository;
            _context = context;
            _accountService = accountService;
        }

        public async Task<Response<IEnumerable<ClubViewModel>>> Handle(GetAllClubsQuery request, CancellationToken cancellationToken)
        {
            var clubs = await _clubRepository.GetAllAsync();
            var clubIds = clubs.Select(c => c.Id).ToList();

            // Fetch official Presidents (Role ID 1) for all clubs
            var presidents = await _context.UserClubs
                .Where(uc => clubIds.Contains(uc.ClubId) && uc.ClubRoleId == 1 && uc.IsActive)
                .ToListAsync(cancellationToken);

            var presidentUserIds = presidents.Select(p => p.UserId).Distinct().ToList();
            var presidentUsers = await _accountService.GetUserNamesAsync(presidentUserIds);

            var luckyPresidentsDict = presidents
                .GroupBy(p => p.ClubId)
                .ToDictionary(
                    g => g.Key, 
                    g => presidentUsers.TryGetValue(g.First().UserId, out var name) ? name : "Unknown President"
                );

            var clubViewModels = clubs.Select(c => new ClubViewModel
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                LogoUrl = c.LogoUrl,
                IsActive = c.IsActive,
                TotalBudget = c.TotalBudget,
                Created = c.Created,
                CreatedBy = c.CreatedBy,
                LeaderName = luckyPresidentsDict.TryGetValue(c.Id, out var boss) ? boss : "Unknown Leader"
            });

            return new Response<IEnumerable<ClubViewModel>>(clubViewModels);
        }
    }
}
