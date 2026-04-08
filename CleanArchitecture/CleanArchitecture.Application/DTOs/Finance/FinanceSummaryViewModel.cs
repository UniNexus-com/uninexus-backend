using System.Collections.Generic;

namespace CleanArchitecture.Core.DTOs.Finance
{
    public class FinanceSummaryViewModel
    {
        public decimal TotalBudget { get; set; }
        public IEnumerable<BudgetRequestViewModel> Requests { get; set; }
    }
}
