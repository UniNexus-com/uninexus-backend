using System.Collections.Generic;

namespace CleanArchitecture.Core.DTOs.Roles
{
    public class ClubRoleViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Color { get; set; }
        public bool IsSystemRole { get; set; }
        public int? ClubId { get; set; }
        public IEnumerable<int> PrivilegeIds { get; set; } = new List<int>();
    }
}
