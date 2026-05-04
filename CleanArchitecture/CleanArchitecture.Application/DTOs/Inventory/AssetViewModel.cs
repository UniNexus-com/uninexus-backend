namespace CleanArchitecture.Core.DTOs.Inventory
{
    public class AssetViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public string Condition { get; set; }
        public string Location { get; set; }
        public string Status { get; set; }
        public decimal Value { get; set; }
        public string SerialNo { get; set; }
        public string Description { get; set; }
        public int? ClubId { get; set; }
        public string ClubName { get; set; }
        public string LoanedBy { get; set; }
        public string LoanedByUserId { get; set; }

        /// <summary>"Borrowed-by-me" listesinde dolu; ödünç alınma tarihi.</summary>
        public System.DateTime? BorrowedAt { get; set; }

        /// <summary>İade için son tarih (varsayılan +7 gün).</summary>
        public System.DateTime? DueDate { get; set; }

        /// <summary>DueDate geçmişse true.</summary>
        public bool IsOverdue { get; set; }

        /// <summary>Loan kayıt durumu (Active / Overdue / Returned).</summary>
        public string LoanStatus { get; set; }
    }
}
