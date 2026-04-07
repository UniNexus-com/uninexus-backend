using System;

namespace CleanArchitecture.Core.DTOs.Clubs
{
    public class MemberDetailsDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string StudentNumber { get; set; }
        public string Role { get; set; }
        public string RoleColor { get; set; }
        public bool IsPresident { get; set; }
        public DateTime Joined { get; set; }
        public string Phone { get; set; }
        public string Major { get; set; }
        public string Year { get; set; }
        public string Bio { get; set; }
    }
}
