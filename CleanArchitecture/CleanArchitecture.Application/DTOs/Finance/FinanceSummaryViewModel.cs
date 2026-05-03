using System.Collections.Generic;

namespace CleanArchitecture.Core.DTOs.Finance
{
    public class FinanceSummaryViewModel
    {
        public decimal TotalBudget { get; set; }
        public decimal TotalRequestedAmount { get; set; }
        public decimal TotalApprovedAmount { get; set; }
    }
}
