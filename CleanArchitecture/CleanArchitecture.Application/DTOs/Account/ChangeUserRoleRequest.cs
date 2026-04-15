namespace CleanArchitecture.Core.DTOs.Account
{
    public class ChangeUserRoleRequest
    {
        public string Role { get; set; }  // "STUDENT" or "CLUB_LEADER"
        public int? ClubId { get; set; }  // Required when Role == "CLUB_LEADER"
    }
}
