namespace CleanArchitecture.Core.DTOs.Account
{
    public class UserAdminDto
    {
        public string Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string StudentNumber { get; set; }
        public string Role { get; set; } // "STUDENT" | "CLUB_LEADER" | "SKS_ADMIN"
        public string Status { get; set; } // "Active" | "Suspended"
    }
}
