using CleanArchitecture.Core.DTOs.Account;
using CleanArchitecture.Core.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using System.Linq;
using System.Threading.Tasks;

namespace CleanArchitecture.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;
        private readonly IWebHostEnvironment _env;
        public AccountController(IAccountService accountService, IWebHostEnvironment env)
        {
            _accountService = accountService;
            _env = env;
        }
        [HttpPost("authenticate")]
        public async Task<IActionResult> AuthenticateAsync([FromBody] AuthenticationRequest request)
        {
            var result = await _accountService.AuthenticateAsync(request, GetIPAddress());
            SetRefreshTokenCookie(result.RefreshToken);
            result.RefreshToken = null; // Don't return the refresh token in the response body
            return Ok(result);
        }
        [HttpPost("register")]
        public async Task<IActionResult> RegisterAsync([FromBody] RegisterRequest request)
        {
            var origin = Request.Headers["origin"].FirstOrDefault() ?? "https://localhost:9001";
            return Ok(await _accountService.RegisterAsync(request, origin));
        }
        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmailAsync([FromQuery] string userId, [FromQuery] string code)
        {
            var origin = Request.Headers["origin"].FirstOrDefault() ?? "https://localhost:9001";
            return Ok(await _accountService.ConfirmEmailAsync(userId, code));
        }
        [HttpPost("logout")]
        public async Task<IActionResult> LogoutAsync()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            if (string.IsNullOrEmpty(refreshToken))
                return BadRequest(new { message = "Refresh token not found" });

            await _accountService.LogoutAsync(refreshToken, GetIPAddress());
            DeleteRefreshTokenCookie();
            return Ok(new { message = "Logged out successfully." });
        }
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPasswordAsync([FromBody] ForgotPasswordRequest model)
        {
            var origin = Request.Headers["origin"].FirstOrDefault() ?? "https://localhost:9001";
            await _accountService.ForgotPasswordAsync(model, origin);
            return Ok(new { message = "Password reset link has been sent to your email address" });
        }
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPasswordAsync([FromBody] ResetPasswordRequest model)
        {

            return Ok(await _accountService.ResetPasswordAsync(model));
        }
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshTokenAsync()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            if (string.IsNullOrEmpty(refreshToken))
                return BadRequest(new { message = "Refresh token not found" });

            var result = await _accountService.RefreshTokenAsync(refreshToken, GetIPAddress());
            SetRefreshTokenCookie(result.RefreshToken);
            result.RefreshToken = null; // Don't return the refresh token in the response body
            return Ok(result);
        }
        private string GetIPAddress()
        {
            if (Request.Headers.ContainsKey("X-Forwarded-For"))
                return Request.Headers["X-Forwarded-For"].FirstOrDefault() ?? "unknown";
            return HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? "unknown";
        }
        private void SetRefreshTokenCookie(string token)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,  // JS erişemez
                Expires = System.DateTime.UtcNow.AddDays(7),
                SameSite = SameSiteMode.Strict, // CSRF koruması
                Secure = _env.EnvironmentName != "Development" // Sadece prod ortamında secure yap
            };
            Response.Cookies.Append("refreshToken", token, cookieOptions);
        }
        private void DeleteRefreshTokenCookie()
        {
            Response.Cookies.Delete("refreshToken", new CookieOptions
            {
                HttpOnly = true,
                SameSite = SameSiteMode.Strict,
                Secure = _env.EnvironmentName != "Development"
            });
        }
    }
}