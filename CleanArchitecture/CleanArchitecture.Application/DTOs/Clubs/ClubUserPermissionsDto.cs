using System.Collections.Generic;

namespace CleanArchitecture.Core.DTOs.Clubs
{
    public class ClubUserPermissionsDto
    {
        public int ClubId { get; set; }
        public string Status { get; set; }
        public string Role { get; set; }
        public bool IsPresident { get; set; }
        public List<string> Privileges { get; set; } = new List<string>();
    }
}
