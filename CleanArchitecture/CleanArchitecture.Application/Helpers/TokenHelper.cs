using System;
using System.Security.Cryptography;
using System.Text;

namespace CleanArchitecture.Core.Helpers
{
    public static class TokenHelper
    {
        public static string GenerateRawToken()
        {
            var randomBytes = new byte[40];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }
        public static string HashToken(string token)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
            return Convert.ToBase64String(bytes);
        }
    }
}
