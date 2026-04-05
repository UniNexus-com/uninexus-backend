using System.Collections.Generic;

namespace CleanArchitecture.Core.Entities
{
    public class Club : AuditableBaseEntity
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string LogoUrl { get; set; }
        public bool IsActive { get; set; }

        public ICollection<Event> Events { get; set; } = new List<Event>();
        public ICollection<UserClub> UserClubs { get; set; } = new List<UserClub>();
        public ICollection<ClubRole> CustomRoles { get; set; } = new List<ClubRole>();
        public ICollection<ClubJoinRequest> JoinRequests { get; set; } = new List<ClubJoinRequest>();
    }
}
