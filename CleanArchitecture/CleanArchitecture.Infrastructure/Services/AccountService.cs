using CleanArchitecture.Core.DTOs.Account;
using CleanArchitecture.Core.DTOs.Email;
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

            if (request.LoginType == "CLUB_LEADER" && !userRoles.Contains(Roles.CLUB_LEADER.ToString()))
                throw new ApiException("This account does not have leader privileges.");
            if (request.LoginType == "SKS_ADMIN" && !userRoles.Contains(Roles.SKS_ADMIN.ToString()))
                throw new ApiException("This account does not have administrator privileges.");

            var jwtToken = await GenerateJWToken(user, userRoles);
            var rawToken = TokenHelper.GenerateRawToken();

            var refreshToken = new RefreshToken
            {
                TokenHash = TokenHelper.HashToken(rawToken),
                Platform = request.LoginType,
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
