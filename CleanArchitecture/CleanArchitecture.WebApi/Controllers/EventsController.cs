using CleanArchitecture.Core.Features.Events.Commands.CheckInEvent;
using CleanArchitecture.Core.Features.Events.Commands.CreateEvent;
using CleanArchitecture.Core.Features.Events.Commands.DeleteEvent;
using CleanArchitecture.Core.Features.Events.Commands.MarkAttendance;
using CleanArchitecture.Core.Features.Events.Commands.RegisterToEvent;
using CleanArchitecture.Core.Features.Events.Commands.UpdateEvent;
using CleanArchitecture.Core.Features.Events.Queries.GetAllEvents;
using CleanArchitecture.Core.Features.Events.Queries.GetPagedEvents;
using CleanArchitecture.Core.Features.Events.Queries.GetEventAttendees;
using CleanArchitecture.Core.Features.Events.Queries.GetEventById;
using CleanArchitecture.Core.Features.Student.Queries.GetTranscript;
using CleanArchitecture.Core.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace CleanArchitecture.WebApi.Controllers
{
    [Authorize]
    public class EventsController : BaseApiController
    {
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] GetAllEventsParameter filter)
        {
            return Ok(await Mediator.Send(new GetAllEventsQuery() { ClubId = filter.ClubId }));
        }

        [HttpGet("paged")]
        public async Task<IActionResult> GetPaged([FromQuery] GetPagedEventsQuery query)
        {
            return Ok(await Mediator.Send(query));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            return Ok(await Mediator.Send(new GetEventByIdQuery { Id = id }));
        }

        [HttpPost]
        public async Task<IActionResult> Post(CreateEventCommand command)
        {
            return Ok(await Mediator.Send(command));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, UpdateEventCommand command)
        {
            if (id != command.Id)
            {
                return BadRequest();
            }
            return Ok(await Mediator.Send(command));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            return Ok(await Mediator.Send(new DeleteEventCommand { Id = id }));
        }

        [HttpPost("{id}/checkin")]
        public async Task<IActionResult> CheckIn(int id, [FromQuery] double? latitude, [FromQuery] double? longitude)
        {
            var command = new CheckInToEventCommand
            {
                EventId = id,
                UserLatitude = latitude,
                UserLongitude = longitude
            };
            return Ok(await Mediator.Send(command));
        }

        // ── Attendee Endpoints ──────────────────────────────────────────

        /// <summary>GET /v1/Events/{id}/attendees — Liste tüm kayıtlı katılımcılar</summary>
        [HttpGet("{id}/attendees")]
        public async Task<IActionResult> GetAttendees(int id)
        {
            return Ok(await Mediator.Send(new GetEventAttendeesQuery { EventId = id }));
        }

        /// <summary>POST /v1/Events/{id}/register — Etkinliğe kayıt ol</summary>
        [HttpPost("{id}/register")]
        public async Task<IActionResult> Register(int id, [FromQuery] double? latitude, [FromQuery] double? longitude)
        {
            var command = new RegisterToEventCommand
            {
                EventId = id,
                UserLatitude = latitude,
                UserLongitude = longitude
            };
            return Ok(await Mediator.Send(command));
        }

        /// <summary>PUT /v1/Events/{id}/attendees/{userId}/attendance — Klüp lideri katılım işaretle</summary>
        [HttpPut("{id}/attendees/{userId}/attendance")]
        public async Task<IActionResult> MarkAttendance(int id, string userId, [FromBody] MarkAttendanceCommand command)
        {
            command.EventId = id;
            command.UserId = userId;
            return Ok(await Mediator.Send(command));
        }

        // ── Transcript ──────────────────────────────────────────────────

        [HttpGet("transcript")]
        public async Task<IActionResult> DownloadTranscript()
        {
            var query = new GetTranscriptQuery { UserId = User.Claims.FirstOrDefault(c => c.Type == "uid")?.Value };
            var response = await Mediator.Send(query);

            if (response.Data == null) return NotFound("Student profile not found.");

            var pdfBytes = TranscriptPdfGenerator.Generate(response.Data);
            return File(pdfBytes, "application/pdf", $"Transcript_{response.Data.StudentNumber}.pdf");
        }

        [HttpGet("transcript/{userId}")]
        public async Task<IActionResult> DownloadTranscript(string userId)
        {
            var currentUserId = User.Claims.FirstOrDefault(c => c.Type == "uid")?.Value;
            
            // Checking for admin or leader roles robustly
            var isAdmin = User.IsInRole("SKS_ADMIN") || User.Claims.Any(c => c.Type == "roles" && c.Value == "SKS_ADMIN");
            var isLeader = User.IsInRole("CLUB_LEADER") || User.Claims.Any(c => c.Type == "roles" && c.Value == "CLUB_LEADER");

            if (currentUserId != userId && !isAdmin && !isLeader)
            {
                return Forbid();
            }

            var query = new GetTranscriptQuery { UserId = userId };
            var response = await Mediator.Send(query);

            if (response.Data == null) return NotFound("Student not found.");

            var pdfBytes = TranscriptPdfGenerator.Generate(response.Data);
            return File(pdfBytes, "application/pdf", $"Transcript_{response.Data.StudentNumber}.pdf");
        }
    }
}
