using CleanArchitecture.Core.DTOs.Account;
using CleanArchitecture.Core.DTOs.Email;
using CleanArchitecture.Core.Wrappers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CleanArchitecture.Core.Interfaces
{
    public interface IAccountService
    {
        Task<AuthenticationResponse> AuthenticateAsync(AuthenticationRequest request, string ipAddress);
        Task<string> RegisterAsync(RegisterRequest request, string origin);
        Task<string> ConfirmEmailAsync(string userId, string code);
        Task LogoutAsync(string refreshToken, string ipAddress);
        Task ForgotPasswordAsync(ForgotPasswordRequest model, string origin);
        Task<string> ResetPasswordAsync(ResetPasswordRequest model);
        Task<AuthenticationResponse> RefreshTokenAsync(string token, string ipAddress);
        Task<Dictionary<string, string>> GetUserNamesAsync(IEnumerable<string> userIds);
    }
}
