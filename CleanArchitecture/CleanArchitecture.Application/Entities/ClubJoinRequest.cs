using System;
using CleanArchitecture.Core.Enums;

namespace CleanArchitecture.Core.Entities
{
    public class ClubJoinRequest : AuditableBaseEntity
    {
        public string UserId { get; set; }
        public int ClubId { get; set; }
        public ClubJoinStatus Status { get; set; }
        public string ProcessedBy { get; set; }
        public DateTime? ProcessedDate { get; set; }

        public Club Club { get; set; }
    }
}
