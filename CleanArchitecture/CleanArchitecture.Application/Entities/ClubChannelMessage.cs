using System;

namespace CleanArchitecture.Core.Entities
{
    public class ClubChannelMessage : AuditableBaseEntity
    {
        public int ChannelId { get; set; }
        public string SenderId { get; set; }
        public string Content { get; set; }
        public DateTime SentAt { get; set; }

        public ClubChannel Channel { get; set; }
        public ApplicationUser Sender { get; set; }
    }
}
