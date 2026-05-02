using System;

namespace CleanArchitecture.Core.DTOs.Clubs
{
    public class ClubMemberDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public int? RoleId { get; set; }
        public string RoleColor { get; set; }
        public bool IsPresident { get; set; }
        public string StudentNumber { get; set; }
        public string Status { get; set; }
        public DateTime Joined { get; set; }
    }
}
