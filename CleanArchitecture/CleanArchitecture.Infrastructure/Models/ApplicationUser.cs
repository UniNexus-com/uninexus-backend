using CleanArchitecture.Core.Entities;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace CleanArchitecture.Infrastructure.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; }
        public string? StudentNumber { get; set; }
        public List<RefreshToken> RefreshTokens { get; set; } = new();
    }
}
