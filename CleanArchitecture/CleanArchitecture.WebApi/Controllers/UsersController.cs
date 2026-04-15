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

        public UsersController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        // GET api/v1/Users
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var users = await _accountService.GetAllUsersAsync();
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
    }
}
