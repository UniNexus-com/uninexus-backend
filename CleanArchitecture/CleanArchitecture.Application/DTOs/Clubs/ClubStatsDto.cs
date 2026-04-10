using System.Collections.Generic;

namespace CleanArchitecture.Core.DTOs.Clubs
{
    public class ClubStatsDto
    {
        public int TotalMembers { get; set; }
        public int UpcomingEventsCount { get; set; }
        public int TotalActivityPoints { get; set; }
        public double GrowthRate { get; set; }
        public decimal TotalBudget { get; set; }
        public List<ActivityPointDto> ActivityLogs { get; set; } = new List<ActivityPointDto>();
    }
}
