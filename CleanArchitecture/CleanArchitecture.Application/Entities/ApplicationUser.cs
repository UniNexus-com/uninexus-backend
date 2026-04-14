using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace CleanArchitecture.Core.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; }
        public string? StudentNumber { get; set; }
        public int ScoreWalletBalance { get; set; } = 0;
        public List<RefreshToken> RefreshTokens { get; set; } = new();
        public ICollection<UserClub> UserClubs { get; set; } = new List<UserClub>();
        public ICollection<ClubJoinRequest> JoinRequests { get; set; } = new List<ClubJoinRequest>();
        public ICollection<EventAttendee> EventAttendees { get; set; } = new List<EventAttendee>();
    }
}
