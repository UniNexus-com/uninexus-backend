using CleanArchitecture.Core.DTOs.Account;
using CleanArchitecture.Core.DTOs.Clubs;
using CleanArchitecture.Core.DTOs.Email;
using CleanArchitecture.Core.DTOs.LeaderbordUserDto;
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
        Task<IEnumerable<UserAdminDto>> GetAllUsersAsync();
        Task<string> ChangeUserRoleAsync(string userId, string newRole, int? clubId);
        Task<string> SuspendUserAsync(string userId);
        Task<List<LeaderboardUserDto>> GetLeaderboardAsync(int limit = 50);
        Task<List<LeaderboardUserDto>> GetClubLeaderboardAsync(int clubId, int limit = 10);
        Task<IEnumerable<UserAdminDto>> SearchUsersAsync(string query);
        Task<MemberDetailsDto> GetUserDetailsAsync(string userId);
    }
}
