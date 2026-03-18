using System;

namespace CleanArchitecture.Core.Entities
{
    public class RefreshToken
    {
        public int Id { get; set; }
        public string TokenHash { get; set; }  // SHA256 hash
        public string ApplicationUserId { get; set; }
        public string Platform { get; set; }  // "Web" veya "Mobile"
        public DateTime ExpiresAt { get; set; }
        public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
        public DateTime CreatedAt { get; set; }
        public string CreatedByIp { get; set; }
        public DateTime? RevokedAt { get; set; }
        public string RevokedByIp { get; set; }
        public string ReplacedByToken { get; set; }
        public bool IsActive => RevokedAt == null && !IsExpired;
    }
}
