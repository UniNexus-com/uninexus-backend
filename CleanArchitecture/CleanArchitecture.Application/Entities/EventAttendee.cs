using System;

namespace CleanArchitecture.Core.Entities
{
    public class EventAttendee : AuditableBaseEntity
    {
        public string UserId { get; set; }
        public int EventId { get; set; }
        public string Status { get; set; } // Interested, Going, Attended
        public Event Event { get; set; }
        public int PointsEarned { get; set; }
        public DateTime CheckInTime { get; set; }
    }
}
