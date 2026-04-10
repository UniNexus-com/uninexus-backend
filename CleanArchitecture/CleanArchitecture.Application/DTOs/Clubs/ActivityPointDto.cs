using System;

namespace CleanArchitecture.Core.DTOs.Clubs
{
    public class ActivityPointDto
    {
        public DateTime Date { get; set; }
        public int Count { get; set; }
        public string Type { get; set; } // "MemberJoined", "EventCreated", "Attendance"
        public string Description { get; set; }
    }
}
