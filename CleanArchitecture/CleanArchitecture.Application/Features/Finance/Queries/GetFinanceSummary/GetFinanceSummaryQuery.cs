using AutoMapper;
using CleanArchitecture.Core.DTOs.Finance;
using CleanArchitecture.Core.Entities;
using CleanArchitecture.Core.Interfaces;
using CleanArchitecture.Core.Wrappers;
using MediatR;
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
        private readonly IMapper _mapper;

        public GetFinanceSummaryQueryHandler(
            IGenericRepositoryAsync<BudgetRequest> budgetRepo,
            IGenericRepositoryAsync<Club> clubRepo,
            IMapper mapper)
        {
            _budgetRepo = budgetRepo;
            _clubRepo = clubRepo;
            _mapper = mapper;
        }

        public async Task<Response<FinanceSummaryViewModel>> Handle(GetFinanceSummaryQuery request, CancellationToken cancellationToken)
        {
            var allRequests = await _budgetRepo.GetAllAsync();
            var filtered = request.ClubId.HasValue
                ? allRequests.Where(r => r.ClubId == request.ClubId)
                : allRequests;

            decimal totalBudget = 0;
            if (request.ClubId.HasValue)
            {
                var allClubs = await _clubRepo.GetAllAsync();
                var club = allClubs.FirstOrDefault(c => c.Id == request.ClubId.Value);
                totalBudget = club?.TotalBudget ?? 0;
            }

            var summary = new FinanceSummaryViewModel
            {
                TotalBudget = totalBudget,
                Requests = _mapper.Map<IEnumerable<BudgetRequestViewModel>>(filtered)
            };

            return new Response<FinanceSummaryViewModel>(summary);
        }
    }
}
