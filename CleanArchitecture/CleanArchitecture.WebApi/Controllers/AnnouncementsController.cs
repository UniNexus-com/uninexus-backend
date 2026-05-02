using CleanArchitecture.Core.DTOs.Announcement;
using CleanArchitecture.Core.Features.Announcements.Commands;
using CleanArchitecture.Core.Features.Announcements.Queries;
using CleanArchitecture.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace CleanArchitecture.WebApi.Controllers
{
    [Authorize]
    public class AnnouncementsController : BaseApiController
    {
        private readonly INotificationService _notificationService;

        public AnnouncementsController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAnnouncement()
        {
            var query = new GetAnnouncementsQuery();
            var result = await Mediator.Send(query);

            return Ok(result.Data);
        }

        [HttpGet("paged")]
        public async Task<IActionResult> GetPaged([FromQuery] GetPagedAnnouncementsQuery query)
        {
            return Ok(await Mediator.Send(query));
        }

        [Authorize(Roles = "SKS_ADMIN")]
        [HttpPost("broadcast")]
        public async Task<IActionResult> BroadCastAnnouncement([FromBody] AnnouncementRequest request)
        {
            var command = new CreateGlobalAnnouncementCommand
            {
                Title = request.Title,
                Message = request.Message,
                Priority = request.Priority
            };

            var result = await Mediator.Send(command);

            if (!result.Succeeded) return BadRequest(result);

            await _notificationService.BroadcastMessageAsync(request.Title, request.Message);

            return Ok(new { Success = true, Message = "Announcement broadcasted successfully." });
        }

        [HttpPost("club/{clubId}/broadcast")]
        [Authorize(Roles = "CLUB_LEADER")]
        public async Task<IActionResult> BroadCastClubAnnouncement(int clubId, [FromBody] AnnouncementRequest request)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == "uid")?.Value;

            var command = new CreateClubAnnouncementCommand
            {
                ClubId = clubId,
                Title = request.Title,
                Message = request.Message,
                Priority = request.Priority,
                CurrentUserId = userId
            };

            var result = await Mediator.Send(command);

            if (!result.Succeeded) return BadRequest(result);

            // TODO: SignalR ile sadece o kulübün üyelerine bildirim gönderme mantığı eklenebilir
            await _notificationService.BroadcastToGroupAsync(clubId.ToString(), request.Title, request.Message);

            return Ok(new { Success = true, Message = "The club announcement has been successfully published." });
        }

        [HttpGet("club/{clubId}")]
        [Authorize]
        public async Task<IActionResult> GetClubAnnouncements(int clubId)
        {
            var query = new GetClubAnnouncementsQuery { ClubId = clubId };
            var result = await Mediator.Send(query);

            return Ok(result.Data);
        }

    }
}
