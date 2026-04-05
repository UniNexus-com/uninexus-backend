using System;

namespace CleanArchitecture.Core.Entities
{
    public class UserClub : AuditableBaseEntity
    {
        public string UserId { get; set; }
        public int ClubId { get; set; }
        public int ClubRoleId { get; set; }
        public DateTime JoinDate { get; set; }
        public bool IsActive { get; set; }

        public Club Club { get; set; }
        public ClubRole Role { get; set; }
    }
}
