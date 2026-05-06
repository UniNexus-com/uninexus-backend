using System;

namespace CleanArchitecture.Core.DTOs.Clubs
{
    public class GatheringClubRequestDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string AdvisorName { get; set; }
        public string RequesterUserId { get; set; }
        public string RequesterName { get; set; }
        public int SupporterCount { get; set; }
        public int MaxSupporters { get; set; } = 2;
        public bool IsSupportedByMe { get; set; }
        public DateTime Created { get; set; }
    }
}
