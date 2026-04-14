using System;

namespace CleanArchitecture.Core.DTOs.Event
{
    public class EventAttendeeDto
    {
        public string UserId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Status { get; set; }
        public DateTime RegisteredAt { get; set; }
        public DateTime? CheckInTime { get; set; }
    }
}
