using CleanArchitecture.Core.DTOs.Account;
using CleanArchitecture.Core.Interfaces;
using CleanArchitecture.Core.Features.Users.Queries.GetRecentActivities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Threading;

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
        public async Task<IActionResult> GetAll([FromQuery] GetPagedUsersRequest request, CancellationToken cancellationToken)
        {
            var result = await _accountService.GetPagedUsersAsync(request, cancellationToken);
            return Ok(result);
        }

        // GET api/v1/Users/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var user = await _accountService.GetUserDetailsAsync(id);
            if (user == null) return NotFound();
            return Ok(new CleanArchitecture.Core.Wrappers.Response<CleanArchitecture.Core.DTOs.Clubs.MemberDetailsDto>(user));
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

        [HttpPut("{id}/suspend")]
        [Authorize(Roles = "SKS_ADMIN")]
        public async Task<IActionResult> Suspend(string id)
        {
            var result = await _accountService.SuspendUserAsync(id);
            return Ok(new { message = "User suspended successfully.", userId = result });
        }

        // PUT api/v1/Users/{id}/activate
        [HttpPut("{id}/activate")]
        [Authorize(Roles = "SKS_ADMIN")]
        public async Task<IActionResult> Activate(string id)
        {
            var result = await _accountService.ActivateUserAsync(id);
            return Ok(new { message = "User activated successfully.", userId = result });
        }

        // GET api/v1/Users/{id}/activities
        [HttpGet("{id}/activities")]
        public async Task<IActionResult> GetActivities(string id)
        {
            return Ok(await Mediator.Send(new GetRecentActivitiesQuery { UserId = id }));
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
