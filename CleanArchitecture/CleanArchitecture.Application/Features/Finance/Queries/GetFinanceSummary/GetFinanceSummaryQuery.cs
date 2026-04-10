using AutoMapper;
using CleanArchitecture.Core.DTOs.Finance;
using CleanArchitecture.Core.Entities;
using CleanArchitecture.Core.Interfaces;
using CleanArchitecture.Core.Wrappers;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CleanArchitecture.Core.Features.Finance.Queries.GetFinanceSummary
{
    public class GetFinanceSummaryQuery : IRequest<Response<FinanceSummaryViewModel>>
    {
        public int? ClubId { get; set; }
    }

    public class GetFinanceSummaryQueryHandler : IRequestHandler<GetFinanceSummaryQuery, Response<FinanceSummaryViewModel>>
    {
        private readonly IGenericRepositoryAsync<BudgetRequest> _budgetRepo;
        private readonly IGenericRepositoryAsync<Club> _clubRepo;
        private readonly IApplicationDbContext _context;
        private readonly IAccountService _accountService;
        private readonly IMapper _mapper;

        public GetFinanceSummaryQueryHandler(
            IGenericRepositoryAsync<BudgetRequest> budgetRepo,
            IGenericRepositoryAsync<Club> clubRepo,
            IApplicationDbContext context,
            IAccountService accountService,
            IMapper mapper)
        {
            _budgetRepo = budgetRepo;
            _clubRepo = clubRepo;
            _context = context;
            _accountService = accountService;
            _mapper = mapper;
        }

        public async Task<Response<FinanceSummaryViewModel>> Handle(GetFinanceSummaryQuery request, CancellationToken cancellationToken)
        {
            var allRequests = await _budgetRepo.GetAllAsync();
            var filtered = request.ClubId.HasValue
                ? allRequests.Where(r => r.ClubId == request.ClubId.Value).ToList()
                : allRequests.ToList();

            var clubs = await _clubRepo.GetAllAsync();
            var clubDict = clubs.ToDictionary(c => c.Id, c => c.Name);

            // Fetch official Presidents (Role ID 1) for all relevant clubs
            var clubIds = filtered.Select(r => r.ClubId).Where(id => id.HasValue).Select(id => id.Value).Distinct().ToList();
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

            decimal totalBudget = 0;
            if (request.ClubId.HasValue)
            {
                var club = await _clubRepo.GetByIdAsync(request.ClubId.Value);
                totalBudget = club?.TotalBudget ?? 0;
            }

            var requestViewModels = _mapper.Map<IEnumerable<BudgetRequestViewModel>>(filtered);
            foreach (var vm in requestViewModels)
            {
                if (vm.ClubId.HasValue && clubDict.TryGetValue(vm.ClubId.Value, out var clubName))
                    vm.ClubName = clubName;
                
                if (vm.ClubId.HasValue && luckyPresidentsDict.TryGetValue(vm.ClubId.Value, out var presidentName))
                    vm.CreatedByName = presidentName;
                else
                    vm.CreatedByName = "Unknown Leader";
            }

            var summary = new FinanceSummaryViewModel
            {
                TotalBudget = totalBudget,
                TotalRequestedAmount = filtered.Where(r => r.Status == "PENDING").Sum(r => r.Amount),
                TotalApprovedAmount = filtered.Where(r => r.Status == "APPROVED").Sum(r => r.Amount),
                Requests = requestViewModels
            };

            return new Response<FinanceSummaryViewModel>(summary);
        }
    }
}
