using System;

namespace CleanArchitecture.Core.Entities
{
    public class ClubCreationRequestSupporter
    {
        public int ClubCreationRequestId { get; set; }
        public ClubCreationRequest ClubCreationRequest { get; set; }
        public string UserId { get; set; }
        public DateTime SupportedAt { get; set; }
    }
}
