using System.Collections.Generic;

namespace CleanArchitecture.Core.Entities
{
    public class ClubCreationRequest : AuditableBaseEntity
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string AdvisorName { get; set; }
        public string RequesterUserId { get; set; }
        public string Status { get; set; }
        public string RejectionReason { get; set; }
        public int SupporterCount { get; set; } = 0;
        public ICollection<ClubCreationRequestSupporter> Supporters { get; set; } = new List<ClubCreationRequestSupporter>();
    }
}
