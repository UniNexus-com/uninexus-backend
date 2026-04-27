using System;
using System.Collections.Generic;

namespace CleanArchitecture.Core.DTOs.Clubs
{
    public class MemberDetailsDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string StudentNumber { get; set; }
        public string Role { get; set; }
        public string RoleColor { get; set; }
        public bool IsPresident { get; set; }
        public DateTime Joined { get; set; }
        public string Phone { get; set; }
        public string Major { get; set; }
        public string Year { get; set; }
        public string Bio { get; set; }
        
        // Performance Stats
        public int EventsAttended { get; set; }
        public double Reliability { get; set; }
        public int ProjectsLed { get; set; }
        public string MemberTier { get; set; }
        
        // Activity List
        public List<MemberActivityDto> Activities { get; set; } = new List<MemberActivityDto>();
    }

    public class MemberActivityDto
    {
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public DateTime Date { get; set; }
        public string Type { get; set; } // e.g., "Attendance", "Organization"
        public string Status { get; set; } // e.g., "Attended", "Absent"
    }
}
