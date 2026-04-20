using CleanArchitecture.Core.Features.Clubs.Commands.AcceptJoinRequest;
using CleanArchitecture.Core.Features.Clubs.Commands.TransferPresident;
using CleanArchitecture.Core.Features.Clubs.Commands.RemoveMember;
using CleanArchitecture.Core.Features.Clubs.Commands.UpdateClubStatus;
using CleanArchitecture.Core.Features.Clubs.Commands.DeclineJoinRequest;
using CleanArchitecture.Core.Features.Clubs.Commands.UpdateClubBudget;
using CleanArchitecture.Core.Features.Clubs.Queries.GetAllClubs;
using CleanArchitecture.Core.Features.Clubs.Queries.GetClubJoinRequests;
using CleanArchitecture.Core.Features.Clubs.Queries.GetClubMembers;
using CleanArchitecture.Core.Features.Clubs.Queries.GetMemberDetails;
using CleanArchitecture.Core.Features.Clubs.Queries.GetManagedClubs;
using CleanArchitecture.Core.Features.Clubs.Queries.GetClubStats;
using CleanArchitecture.Core.Features.Clubs.Queries.GetPresidentClubs;
using CleanArchitecture.Core.Features.Roles.Commands.CreateClubRole;
using CleanArchitecture.Core.Features.Roles.Commands.DeleteClubRole;
using CleanArchitecture.Core.Features.Roles.Commands.UpdateMemberRole;
using CleanArchitecture.Core.Features.Roles.Queries.GetClubPrivileges;
using CleanArchitecture.Core.Features.Roles.Queries.GetClubRoles;
using CleanArchitecture.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace CleanArchitecture.WebApi.Controllers
{
    [Authorize]
    public class ClubsController : BaseApiController
    {
        private readonly IClubRepositoryAsync _clubRepository;
        private readonly IAuthenticatedUserService _authenticatedUserService;

        public ClubsController(IClubRepositoryAsync clubRepository, IAuthenticatedUserService authenticatedUserService)
        {
            _clubRepository = clubRepository;
            _authenticatedUserService = authenticatedUserService;
        }

        [HttpGet("managed")]
        public async Task<IActionResult> GetManagedClubs()
        {
            return Ok(await Mediator.Send(new GetManagedClubsQuery()));
        }

        [HttpGet("president-clubs")]
        public async Task<IActionResult> GetPresidentClubs()
        {
            return Ok(await Mediator.Send(new GetPresidentClubsQuery()));
        }

        [HttpGet("{id}/validate-access")]
        public async Task<IActionResult> ValidateClubAccess(int id)
        {
            var userId = _authenticatedUserService.UserId;
            var isPresident = await _clubRepository.IsPresidentOfClubAsync(id, userId);
            if (!isPresident)
                return Forbid();

            var club = await _clubRepository.GetByIdAsync(id);
            if (club == null)
                return NotFound(new { message = "Club not found." });

            if (club.Status == "PENDING")
                return BadRequest(new { code = "CLUB_PENDING", message = "This club is suspended." });
            if (club.Status == "CLOSED")
                return BadRequest(new { code = "CLUB_CLOSED", message = "This club is closed." });

            return Ok(new { clubId = id, status = club.Status });
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await Mediator.Send(new GetAllClubsQuery()));
        }

        [HttpGet("privileges")]
        public async Task<IActionResult> GetPrivileges()
        {
            return Ok(await Mediator.Send(new GetClubPrivilegesQuery()));
        }

        [HttpGet("{id}/members")]
        public async Task<IActionResult> GetMembers(int id)
        {
            return Ok(await Mediator.Send(new GetClubMembersQuery { ClubId = id }));
        }

        [HttpGet("{clubId}/members/{userId}")]
        public async Task<IActionResult> GetMemberDetails(int clubId, string userId)
        {
            return Ok(await Mediator.Send(new GetMemberDetailsQuery { ClubId = clubId, UserId = userId }));
        }

        [HttpDelete("{clubId}/members/{userId}")]
        public async Task<IActionResult> RemoveMember(int clubId, string userId)
            => Ok(await Mediator.Send(new RemoveMemberCommand { ClubId = clubId, UserId = userId }));

        [HttpGet("{id}/join-requests")]
        public async Task<IActionResult> GetJoinRequests(int id)
        {
            return Ok(await Mediator.Send(new GetClubJoinRequestsQuery { ClubId = id }));
        }

        [HttpPost("accept-request/{id}")]
        public async Task<IActionResult> AcceptRequest(int id)
        {
            return Ok(await Mediator.Send(new AcceptJoinRequestCommand { Id = id }));
        }

        [HttpPost("decline-request/{id}")]
        public async Task<IActionResult> DeclineRequest(int id)
        {
            return Ok(await Mediator.Send(new DeclineJoinRequestCommand { Id = id }));
        }

        [HttpGet("{clubId}/roles")]
        public async Task<IActionResult> GetRoles(int clubId)
            => Ok(await Mediator.Send(new GetClubRolesQuery { ClubId = clubId }));

        [HttpPost("{clubId}/roles")]
        public async Task<IActionResult> CreateRole(int clubId, CreateClubRoleCommand command)
        {
            command.ClubId = clubId;
            return Ok(await Mediator.Send(command));
        }

        [HttpDelete("{clubId}/roles/{roleId}")]
        public async Task<IActionResult> DeleteRole(int clubId, int roleId)
            => Ok(await Mediator.Send(new DeleteClubRoleCommand { Id = roleId }));

        [HttpPut("{id}/role")]
        public async Task<IActionResult> UpdateMemberRole(int clubId, string userId, UpdateMemberRoleCommand command)
        {
            command.ClubId = clubId;
            command.UserId = userId;
            return Ok(await Mediator.Send(command));
        }

        [HttpPut("{id}/budget")]
        public async Task<IActionResult> UpdateBudget(int id, UpdateClubBudgetCommand command)
        {
            if (id != command.Id) return BadRequest();
            return Ok(await Mediator.Send(command));
        }

        [HttpGet("{id}/stats")]
        public async Task<IActionResult> GetStats(int id)
        {
            return Ok(await Mediator.Send(new GetClubStatsQuery { ClubId = id }));
        }

        [HttpPut("{id}/status")]
        [Authorize(Roles = "SKS_ADMIN")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateClubStatusCommand command)
        {
            if (id != command.Id) return BadRequest();
            return Ok(await Mediator.Send(command));
        }

        [HttpPut("{clubId}/transfer-president")]
        [Authorize(Roles = "SKS_ADMIN")]
        public async Task<IActionResult> TransferPresident(int clubId, [FromBody] TransferPresidentCommand command)
        {
            command.ClubId = clubId;
            return Ok(await Mediator.Send(command));
        }
    }
}
