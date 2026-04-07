using CleanArchitecture.Core.Features.Clubs.Commands.AcceptJoinRequest;
using CleanArchitecture.Core.Features.Clubs.Commands.DeclineJoinRequest;
using CleanArchitecture.Core.Features.Clubs.Queries.GetClubJoinRequests;
using CleanArchitecture.Core.Features.Clubs.Queries.GetClubMembers;
using CleanArchitecture.Core.Features.Clubs.Queries.GetMemberDetails;
using CleanArchitecture.Core.Features.Clubs.Queries.GetManagedClubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace CleanArchitecture.WebApi.Controllers
{
    [Authorize]
    public class ClubsController : BaseApiController
    {
        [HttpGet("managed")]
        public async Task<IActionResult> GetManagedClubs()
        {
            return Ok(await Mediator.Send(new GetManagedClubsQuery()));
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
    }
}
