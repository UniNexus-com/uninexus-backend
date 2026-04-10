using System;

namespace CleanArchitecture.Core.DTOs.Finance
{
    public class BudgetRequestViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; }
        public string CreatedBy { get; set; }
        public string CreatedByName { get; set; }
        public DateTime Created { get; set; }
        public int? ClubId { get; set; }
        public string ClubName { get; set; }
    }
}
