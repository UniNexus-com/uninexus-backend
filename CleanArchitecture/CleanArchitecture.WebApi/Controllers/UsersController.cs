using CleanArchitecture.Core.DTOs.Account;
using CleanArchitecture.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace CleanArchitecture.WebApi.Controllers
{
    [Authorize]
    public class UsersController : BaseApiController
    {
        private readonly IAccountService _accountService;
        private readonly IClubRepositoryAsync _clubRepository;
        private readonly IAuthenticatedUserService _authenticatedUserService;

        public UsersController(
            IAccountService accountService, 
            IClubRepositoryAsync clubRepository, 
            IAuthenticatedUserService authenticatedUserService)
        {
            _accountService = accountService;
            _clubRepository = clubRepository;
            _authenticatedUserService = authenticatedUserService;
        }

        // GET api/v1/Users
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var users = await _accountService.GetAllUsersAsync();
            return Ok(users);
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string q)
        {
            var users = await _accountService.SearchUsersAsync(q);
            return Ok(users);
        }

        // PUT api/v1/Users/{id}/role
        [HttpPut("{id}/role")]
        [Authorize(Roles = "SKS_ADMIN")]
        public async Task<IActionResult> ChangeRole(string id, [FromBody] ChangeUserRoleRequest request)
        {
            var result = await _accountService.ChangeUserRoleAsync(id, request.Role, request.ClubId);
            return Ok(new { message = "Role updated successfully.", userId = result });
        }

        [HttpGet("leaderboard")]
        public async Task<IActionResult> GetLeaderboard([FromQuery] int limit = 50)
        {
            var leaderboard = await _accountService.GetLeaderboardAsync(limit);
            return Ok(leaderboard);
        }

        [HttpGet("leaderboard/club/{clubId}")]
        public async Task<IActionResult> GetClubLeaderboard(int clubId, [FromQuery] int limit = 10)
        {
            var currentUserId = _authenticatedUserService.UserId;

            var isMember = await _clubRepository.IsClubMemberAsync(clubId, currentUserId);

            if (!isMember)
            {
                return Forbid();
            }

            var leaderboard = await _accountService.GetClubLeaderboardAsync(clubId, limit);
            return Ok(leaderboard);
        }
    }
}
