using System.Collections.Generic;

namespace CleanArchitecture.Core.Entities
{
    public class Club : AuditableBaseEntity
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string LogoUrl { get; set; }
        public bool IsActive { get; set; }
        public string Status { get; set; } = "ACTIVE"; // "ACTIVE" | "PENDING" | "CLOSED"
        public decimal? TotalBudget { get; set; }

        public ICollection<EventClub> EventHostingLinks { get; set; } = new List<EventClub>();
        public ICollection<UserClub> UserClubs { get; set; } = new List<UserClub>();
        public ICollection<ClubRole> CustomRoles { get; set; } = new List<ClubRole>();
        public ICollection<ClubJoinRequest> JoinRequests { get; set; } = new List<ClubJoinRequest>();
        public ICollection<Asset> Assets { get; set; } = new List<Asset>();
        public ICollection<BudgetRequest> BudgetRequests { get; set; } = new List<BudgetRequest>();
        public ICollection<ClubChannel> Channels { get; set; } = new List<ClubChannel>();
    }
}
