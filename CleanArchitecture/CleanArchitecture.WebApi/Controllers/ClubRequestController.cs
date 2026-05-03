using CleanArchitecture.Core.Features.ClubRequests.Commands.CreateClubCreationRequest;
using CleanArchitecture.Core.Features.ClubRequests.Commands.SupportClubCreationRequest;
using CleanArchitecture.Core.Features.ClubRequests.Queries.GetGatheringRequests;
using CleanArchitecture.Core.Features.ClubRequests.Queries.GetMyClubCreationRequest;
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

        // Öğrencinin kendi kulüp kurma talebini görüntülemesi
        [HttpGet("my")]
        public async Task<IActionResult> GetMyRequest()
        {
            return Ok(await Mediator.Send(new GetMyClubCreationRequestQuery()));
        }

        // Destek bekleyen talepleri listeleme (mobil)
        [HttpGet("gathering")]
        public async Task<IActionResult> GetGatheringRequests()
        {
            return Ok(await Mediator.Send(new GetGatheringRequestsQuery()));
        }

        // Bir talebe destek olma (öğrenci)
        [HttpPost("{id}/support")]
        public async Task<IActionResult> SupportRequest(int id)
        {
            return Ok(await Mediator.Send(new SupportClubCreationRequestCommand { RequestId = id }));
        }

        // SKS_ADMIN'in bekleyen (50 destek almış) talepleri görmesi
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