using CleanArchitecture.Core.Interfaces;
using CleanArchitecture.Core.DTOs.Announcement;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace CleanArchitecture.WebApi.Controllers
{
    [Authorize(Roles = "SKS_ADMIN")]
    public class AnnouncementsController : BaseApiController
    {
        private readonly INotificationService _notificationService;

        public AnnouncementsController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpPost("broadcast")]
        public async Task<IActionResult> BroadCastAnnouncement([FromBody] AnnouncementRequest request)
        {
            await _notificationService.BroadcastMessageAsync(request.Title, request.Message);

            return Ok(new { Success = true, Message = "Announcement broadcasted successfully." });
        }

        
    }
}
