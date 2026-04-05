using System.Collections.Generic;

namespace CleanArchitecture.Core.Entities
{
    public class ClubRole : AuditableBaseEntity
    {
        public string Name { get; set; }
        public int? ClubId { get; set; }
        public bool IsSystemRole { get; set; }

        public Club Club { get; set; }
        public ICollection<ClubRolePrivilege> RolePrivileges { get; set; } = new List<ClubRolePrivilege>();
        public ICollection<UserClub> UserClubs { get; set; } = new List<UserClub>();
    }
}
