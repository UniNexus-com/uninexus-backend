#nullable enable
using System;

namespace CleanArchitecture.Core.Entities
{
    public class ChatMessage : AuditableBaseEntity
    {
        public string SenderId { get; set; } = string.Empty;
        public string ReceiverId { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public DateTime SentAt { get; set; }
        public string? Reaction { get; set; }

        public virtual ApplicationUser? Sender { get; set; }
        public virtual ApplicationUser? Receiver { get; set; }
    }
}
