using System;

namespace CleanArchitecture.Core.DTOs.Clubs
{
    public class ClubViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string LogoUrl { get; set; }
        public bool IsActive { get; set; }
        public string Status { get; set; }
        public decimal? TotalBudget { get; set; }
        public int MemberCount { get; set; }
        public DateTime Created { get; set; }
        public string CreatedBy { get; set; }
        public string LeaderName { get; set; }
        public bool IsJoined { get; set; }
        public bool IsPending { get; set; }
    }
}
