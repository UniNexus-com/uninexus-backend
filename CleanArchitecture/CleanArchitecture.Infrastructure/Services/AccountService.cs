using CleanArchitecture.Core.DTOs.Account;
using CleanArchitecture.Core.DTOs.Email;
using CleanArchitecture.Core.DTOs.LeaderbordUserDto;
using CleanArchitecture.Core.Entities;
using CleanArchitecture.Core.Enums;
using CleanArchitecture.Core.Exceptions;
using CleanArchitecture.Core.Helpers;
using CleanArchitecture.Core.Interfaces;
using CleanArchitecture.Core.Settings;
using CleanArchitecture.Core.Wrappers;
using CleanArchitecture.Infrastructure.Contexts;
using CleanArchitecture.Infrastructure.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Infrastructure.Services
{
    public class AccountService : IAccountService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailService _emailService;
        private readonly JWTSettings _jwtSettings;
        private readonly IDateTimeService _dateTimeService;
        private readonly ApplicationDbContext _context;
        public AccountService(UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IOptions<JWTSettings> jwtSettings,
            IDateTimeService dateTimeService,
            SignInManager<ApplicationUser> signInManager,
            IEmailService emailService,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _jwtSettings = jwtSettings.Value;
            _dateTimeService = dateTimeService;
            _signInManager = signInManager;
            _context = context;
            _emailService = emailService;
        }

        // -- Authenticate ---
        public async Task<AuthenticationResponse> AuthenticateAsync(AuthenticationRequest request, string ipAddress)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
                throw new ApiException($"No Accounts Registered with {request.Email}.");

            var isValid = await _userManager.CheckPasswordAsync(user, request.Password);
            if (!isValid)
                throw new ApiException($"Invalid Credentials for '{request.Email}'.");

            if (!user.EmailConfirmed)
                throw new ApiException($"Account Not Confirmed for '{request.Email}'.");

            var userRoles = await _userManager.GetRolesAsync(user);

            // Strict LoginType role checks removed for automatic role detection

            var jwtToken = await GenerateJWToken(user, userRoles);
            var rawToken = TokenHelper.GenerateRawToken();

            var refreshToken = new RefreshToken
            {
                TokenHash = TokenHelper.HashToken(rawToken),
                Platform = request.LoginType ?? "WEB",
                ApplicationUserId = user.Id,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow,
                CreatedByIp = ipAddress
            };

            // CLEANUP: Remove old tokens for this user and platform to prevent bloat
            var existingTokens = await _context.RefreshTokens
                .Where(t => t.ApplicationUserId == user.Id && t.Platform == request.LoginType)
                .ToListAsync();
            
            if (existingTokens.Any())
            {
                _context.RefreshTokens.RemoveRange(existingTokens);
            }

            // Create new session token
            _context.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync();

            return new AuthenticationResponse
            {
                Id = user.Id,
                JWToken = new JwtSecurityTokenHandler().WriteToken(jwtToken),
                Email = user.Email,
                UserName = user.UserName,
                FullName = user.FullName,
                Roles = userRoles.ToList(),
                IsVerified = user.EmailConfirmed,
                RefreshToken = rawToken
            };
        }

        // -- Register ---
        public async Task<string> RegisterAsync(RegisterRequest request, string origin)
        {
            if (await _userManager.FindByNameAsync(request.UserName) != null)
                throw new ApiException($"Username '{request.UserName}' is already taken.");

            if (!request.Email.EndsWith(".edu.tr"))
                throw new ApiException("Only university email addresses are accepted.");

            if (await _userManager.FindByEmailAsync(request.Email) != null)
                throw new ApiException($"Email {request.Email} is already registered.");

            var user = new ApplicationUser
            {
                Email = request.Email,
                FullName = request.FullName,
                UserName = request.UserName,
                StudentNumber = request.StudentNumber
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
                throw new ApiException(string.Join(", ", result.Errors.Select(e => e.Description)));

            await _userManager.AddToRoleAsync(user, Roles.STUDENT.ToString());

            var verificationUri = await BuildConfirmEmailUri(user, origin);
            await _emailService.SendAsync(new EmailRequest
            {
                To = user.Email,
                Subject = "Verify Your Account",
                Body = EmailTemplates.ConfirmEmail(user.FullName, verificationUri)
            });

            return "Register sucess. Please verify your email adress.";
        }

        // ── Confirm Email ─────────────────────────────────────────────
        public async Task<string> ConfirmEmailAsync(string userId, string code)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) throw new ApiException("User not found.");

            code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
            var result = await _userManager.ConfirmEmailAsync(user, code);
            if (!result.Succeeded)
                throw new ApiException($"Email verifying fail: {user.Email}");

            return "Email verified. You can now login the website.";
        }

        // ── Logout (everywhere) ───────────────────────────────────────
        public async Task LogoutAsync(string refreshToken, string ipAddress)
        {
            var tokenHash = TokenHelper.HashToken(refreshToken);

            var user = await _userManager.Users
                .Include(u => u.RefreshTokens)
                .SingleOrDefaultAsync(u => u.RefreshTokens.Any(t => t.TokenHash == tokenHash));

            if (user == null) throw new ApiException("Invalid token.");

            var refreshTokenEntity = user.RefreshTokens.Single(t => t.TokenHash == tokenHash);
            if (!refreshTokenEntity.IsActive) throw new ApiException("Token is already inactive.");

            refreshTokenEntity.RevokedAt = _dateTimeService.NowUtc;
            refreshTokenEntity.RevokedByIp = ipAddress;

            //await _userManager.UpdateAsync(user);
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        // ── Refresh Token ─────────────────────────────────────────────
        public async Task<AuthenticationResponse> RefreshTokenAsync(string token, string ipAddress)
        {
            var tokenHash = TokenHelper.HashToken(token);

            var user = await _userManager.Users
                .Include(u => u.RefreshTokens)
                .SingleOrDefaultAsync(u => u.RefreshTokens.Any(t => t.TokenHash == tokenHash));

            if (user == null) throw new ApiException("Invalid token.");

            var refreshToken = user.RefreshTokens.Single(x => x.TokenHash == tokenHash);
            if (!refreshToken.IsActive) throw new ApiException("Token is inactive.");

            var roles = await _userManager.GetRolesAsync(user);
            var jwtToken = await GenerateJWToken(user, roles);

            return new AuthenticationResponse
            {
                Id = user.Id,
                JWToken = new JwtSecurityTokenHandler().WriteToken(jwtToken),
                Email = user.Email,
                UserName = user.UserName,
                FullName = user.FullName,
                Roles = roles.ToList(),
                IsVerified = user.EmailConfirmed,
                RefreshToken = token // Reuse the same token
            };
        }

        // ── Forgot Password ───────────────────────────────────────────
        public async Task ForgotPasswordAsync(ForgotPasswordRequest model, string origin)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null) return; // enumeration'a karşı sessiz dön

            var code = await _userManager.GeneratePasswordResetTokenAsync(user);
            var encoded = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var resetUrl = $"{origin}/api/account/reset-password?email={Uri.EscapeDataString(user.Email)}&token={encoded}";

            await _emailService.SendAsync(new EmailRequest
            {
                To = user.Email,
                Subject = "Şifre Sıfırlama",
                Body = EmailTemplates.ResetPassword(user.FullName, resetUrl)
            });
        }

        // ── Reset Password ────────────────────────────────────────────
        public async Task<string> ResetPasswordAsync(ResetPasswordRequest model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null) throw new ApiException($"No Accounts Registered with {model.Email}.");

            var token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(model.Token));
            var result = await _userManager.ResetPasswordAsync(user, token, model.Password);
            if (!result.Succeeded)
                throw new ApiException("Şifre sıfırlama başarısız.");

            return "Şifreniz başarıyla sıfırlandı.";
        }

        // ── Private Helpers ───────────────────────────────────────────
        private async Task<JwtSecurityToken> GenerateJWToken(ApplicationUser user, IEnumerable<string> roles = null)
        {
            var userClaims = await _userManager.GetClaimsAsync(user);
            var userRoles = roles ?? await _userManager.GetRolesAsync(user);
            var roleClaims = userRoles.Select(r => new Claim("roles", r));

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub,   user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti,   Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim("uid", user.Id),
                new Claim("ip",  IpHelper.GetIpAddress())
            }
            .Union(userClaims)
            .Union(roleClaims);

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            return new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.DurationInMinutes),
                signingCredentials: credentials);
        }
        public async Task<Dictionary<string, string>> GetUserNamesAsync(IEnumerable<string> userIds)
        {
            return await _userManager.Users
                .Where(u => userIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.FullName);
        }

        public async Task<IEnumerable<UserAdminDto>> GetAllUsersAsync()
        {
            var users = await _userManager.Users.ToListAsync();
            var result = new List<UserAdminDto>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);

                string primaryRole = Roles.STUDENT.ToString();
                if (roles.Contains(Roles.SKS_ADMIN.ToString()))
                    primaryRole = Roles.SKS_ADMIN.ToString();
                else if (roles.Contains(Roles.CLUB_LEADER.ToString()))
                    primaryRole = Roles.CLUB_LEADER.ToString();

                result.Add(new UserAdminDto
                {
                    Id            = user.Id,
                    FullName      = user.FullName,
                    Email         = user.Email,
                    StudentNumber = user.StudentNumber,
                    Role          = primaryRole,
                });
            }

            return result;
        }

        public async Task<string> ChangeUserRoleAsync(string userId, string newRole, int? clubId)
        {
            var allowedRoles = new[] { Roles.STUDENT.ToString(), Roles.CLUB_LEADER.ToString() };
            if (!allowedRoles.Contains(newRole))
                throw new ApiException($"Invalid role. Allowed: {string.Join(", ", allowedRoles)}.");

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) throw new ApiException("User not found.");

            if (newRole == Roles.CLUB_LEADER.ToString())
            {
                if (!clubId.HasValue)
                    throw new ApiException("clubId is required when assigning the CLUB_LEADER role.");

                var club = await _context.Clubs.FirstOrDefaultAsync(c => c.Id == clubId.Value);
                if (club == null) throw new ApiException($"Club with Id {clubId} not found.");

                // Find President role by name (dynamic ID, not hardcoded)
                var presidentRole = await _context.ClubRoles
                    .FirstOrDefaultAsync(r => r.Name == "President" && r.IsSystemRole);
                if (presidentRole == null) throw new ApiException("President role not found in the system.");

                var memberRole = await _context.ClubRoles
                    .FirstOrDefaultAsync(r => r.Name == "Active Member" && r.IsSystemRole);
                if (memberRole == null) throw new ApiException("Active Member role not found in the system.");

                // Find the current president of that club
                var currentPresidentRecord = await _context.UserClubs
                    .FirstOrDefaultAsync(uc => uc.ClubId == clubId.Value
                                            && uc.ClubRoleId == presidentRole.Id
                                            && uc.IsActive);

                if (currentPresidentRecord != null && currentPresidentRecord.UserId != userId)
                {
                    // Demote old president's Identity role
                    var oldPresident = await _userManager.FindByIdAsync(currentPresidentRecord.UserId);
                    if (oldPresident != null)
                    {
                        var oldPresidentRoles = await _userManager.GetRolesAsync(oldPresident);
                        if (oldPresidentRoles.Contains(Roles.CLUB_LEADER.ToString()))
                            await _userManager.RemoveFromRoleAsync(oldPresident, Roles.CLUB_LEADER.ToString());
                        if (!oldPresidentRoles.Contains(Roles.STUDENT.ToString()))
                            await _userManager.AddToRoleAsync(oldPresident, Roles.STUDENT.ToString());
                    }

                    // Downgrade old president's club role to Active Member (keep them as a member)
                    var trackedOldRecord = await _context.UserClubs
                        .AsTracking()
                        .FirstOrDefaultAsync(uc => uc.UserId == currentPresidentRecord.UserId
                                                && uc.ClubId == clubId.Value);
                    if (trackedOldRecord != null)
                    {
                        trackedOldRecord.ClubRoleId = memberRole.Id;
                        _context.UserClubs.Update(trackedOldRecord);
                        await _context.SaveChangesAsync();
                    }
                }

                // Assign new user's Identity role
                var currentUserRoles = await _userManager.GetRolesAsync(user);
                if (currentUserRoles.Contains(Roles.STUDENT.ToString()))
                    await _userManager.RemoveFromRoleAsync(user, Roles.STUDENT.ToString());
                if (!currentUserRoles.Contains(Roles.CLUB_LEADER.ToString()))
                    await _userManager.AddToRoleAsync(user, Roles.CLUB_LEADER.ToString());

                // Create or update new president's UserClub record
                var existingMembership = await _context.UserClubs
                    .AsTracking()
                    .FirstOrDefaultAsync(uc => uc.UserId == userId && uc.ClubId == clubId.Value);

                if (existingMembership != null)
                {
                    existingMembership.ClubRoleId = presidentRole.Id;
                    existingMembership.IsActive = true;
                    _context.UserClubs.Update(existingMembership);
                }
                else
                {
                    await _context.UserClubs.AddAsync(new UserClub
                    {
                        UserId     = userId,
                        ClubId     = clubId.Value,
                        ClubRoleId = presidentRole.Id,
                        JoinDate   = DateTime.UtcNow,
                        IsActive   = true,
                        Created    = DateTime.UtcNow,
                        CreatedBy  = "admin"
                    });
                }

                await _context.SaveChangesAsync();
            }
            else // STUDENT
            {
                var currentUserRoles = await _userManager.GetRolesAsync(user);
                if (currentUserRoles.Contains(Roles.CLUB_LEADER.ToString()))
                    await _userManager.RemoveFromRoleAsync(user, Roles.CLUB_LEADER.ToString());
                if (!currentUserRoles.Contains(Roles.STUDENT.ToString()))
                    await _userManager.AddToRoleAsync(user, Roles.STUDENT.ToString());
            }

            return userId;
        }

        public async Task<List<LeaderboardUserDto>> GetLeaderboardAsync(int limit = 50)
        {
            var users = await _userManager.Users
                .OrderByDescending(u => u.ScoreWalletBalance)
                .Take(limit)
                .Select(u => new LeaderboardUserDto
                {
                    UserId = u.Id,
                    StudentNumber = u.StudentNumber,
                    FullName = u.FullName,
                    ScoreWalletBalance = u.ScoreWalletBalance
                })
                .ToListAsync();

            for (int i = 0; i < users.Count; i++)
            {
                users[i].Rank = i + 1;
            }

            return users;
        }

        public async Task<List<LeaderboardUserDto>> GetClubLeaderboardAsync(int clubId, int limit = 10)
        {
            var clubUsers = await _context.UserClubs
                .Where(uc => uc.ClubId == clubId && uc.IsActive)
                .Join(_userManager.Users,
                    uc => uc.UserId,
                    u => u.Id,
                    (uc, u) => new LeaderboardUserDto
                    {
                        UserId = u.Id,
                        StudentNumber = u.StudentNumber,
                        FullName = u.FullName,
                        ScoreWalletBalance = u.ScoreWalletBalance
                    })
                .OrderByDescending(u => u.ScoreWalletBalance)
                .Take(limit)
                .ToListAsync();

            for (int i = 0; i < clubUsers.Count; i++)
            {
                clubUsers[i].Rank = i + 1;
            }

            return clubUsers;
        }

        private async Task<string> BuildConfirmEmailUri(ApplicationUser user, string origin)
        {
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var encoded = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var uri = new Uri($"{origin}/api/account/confirm-email/");
            var url = QueryHelpers.AddQueryString(uri.ToString(), "userId", user.Id);
            return QueryHelpers.AddQueryString(url, "code", encoded);
        }
    }
}
