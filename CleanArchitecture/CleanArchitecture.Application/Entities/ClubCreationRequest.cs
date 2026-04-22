using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
