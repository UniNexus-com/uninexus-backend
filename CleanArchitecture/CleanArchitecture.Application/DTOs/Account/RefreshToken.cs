using System;

namespace CleanArchitecture.Core.DTOs.Account
{
    public class RefreshToken
    {
        public int Id { get; set; }
        public string Token { get; set; }
        public string ApplicationUserId { get; set; }
        public string Platform { get; set; }  // "Web" veya "Mobile"
        public DateTime Expires { get; set; }
        public bool IsExpired => DateTime.UtcNow >= Expires;
        public DateTime Created { get; set; }
        public string CreatedByIp { get; set; }
        public DateTime? Revoked { get; set; }
        public string RevokedByIp { get; set; }
        public string ReplacedByToken { get; set; }
        public bool IsActive => Revoked == null && !IsExpired;
    }
}
