using System.Collections.Generic;

namespace CleanArchitecture.Core.Entities
{
    public class ClubPrivilege : BaseEntity
    {
        public string Name { get; set; }
        public string Description { get; set; }

        public ICollection<ClubRolePrivilege> RolePrivileges { get; set; } = new List<ClubRolePrivilege>();
    }
}
