using System;

namespace CleanArchitecture.Core.Entities
{
    public class AssetLoan : AuditableBaseEntity
    {
        public int AssetId { get; set; }
        public Asset Asset { get; set; }

        public string UserId { get; set; }

        public DateTime BorrowedAt { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? ReturnedAt { get; set; }

        /// <summary>Active, Returned, Overdue</summary>
        public string Status { get; set; }
    }
}
