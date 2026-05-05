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
            var query = _context.BudgetRequests.AsNoTracking();

            if (request.ClubId.HasValue)
            {
                query = query.Where(r => r.ClubId == request.ClubId.Value);
            }

            decimal totalBudget = 0;
            if (request.ClubId.HasValue)
            {
                var club = await _context.Clubs
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Id == request.ClubId.Value, cancellationToken);
                totalBudget = club?.TotalBudget ?? 0;
            }

            var requests = await query
                .OrderByDescending(r => r.Created)
                .ToListAsync(cancellationToken);

            var summary = new FinanceSummaryViewModel
            {
                TotalBudget = totalBudget,
                TotalRequestedAmount = await query.Where(r => r.Status == "PENDING").SumAsync(r => r.Amount, cancellationToken),
                TotalApprovedAmount = await query.Where(r => r.Status == "APPROVED").SumAsync(r => r.Amount, cancellationToken),
                Requests = _mapper.Map<List<BudgetRequestViewModel>>(requests)
            };

            return new Response<FinanceSummaryViewModel>(summary);
        }
    }
}
