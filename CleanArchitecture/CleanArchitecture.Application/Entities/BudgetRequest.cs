namespace CleanArchitecture.Core.Entities
{
    public class BudgetRequest : AuditableBaseEntity
    {
        public string Title { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; }
        public int? ClubId { get; set; }
        public Club Club { get; set; }
    }
}
