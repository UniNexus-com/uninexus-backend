using CleanArchitecture.Core.Features.ClubRequests.Commands.CreateClubCreationRequest;
using CleanArchitecture.Core.Features.Clubs.Commands.ApproveClubCreationRequest;
using CleanArchitecture.Core.Features.Clubs.Commands.RejectClubCreationRequest;
using CleanArchitecture.Core.Features.Clubs.Queries.GetPendingClubRequests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace CleanArchitecture.WebApi.Controllers
{
    [Authorize]
    public class ClubRequestsController : BaseApiController
    {
        // Öğrencinin kulüp kurma talebi atması
        [HttpPost]
        public async Task<IActionResult> CreateRequest(CreateClubCreationRequestCommand command)
        {
            return Ok(await Mediator.Send(command));
        }

        // SKS_ADMIN'in bekleyen talepleri görmesi
        [HttpGet("pending")]
        [Authorize(Roles = "SKS_ADMIN")]
        public async Task<IActionResult> GetPendingRequests()
        {
            return Ok(await Mediator.Send(new GetPendingClubRequestsQuery()));
        }

        // SKS_ADMIN'in talebi onaylaması
        [HttpPut("{id}/approve")]
        [Authorize(Roles = "SKS_ADMIN")]
        public async Task<IActionResult> ApproveRequest(int id)
        {
            return Ok(await Mediator.Send(new ApproveClubCreationRequestCommand { RequestId = id }));
        }

        // SKS_ADMIN'in talebi reddetmesi
        [HttpPut("{id}/reject")]
        [Authorize(Roles = "SKS_ADMIN")]
        public async Task<IActionResult> RejectRequest(int id, [FromBody] string reason)
        {
            return Ok(await Mediator.Send(new RejectClubCreationRequestCommand { RequestId = id, RejectionReason = reason }));
        }
    }
}