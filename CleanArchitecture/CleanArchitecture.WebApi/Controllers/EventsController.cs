using CleanArchitecture.Core.Features.Events.Commands.CreateEvent;
using CleanArchitecture.Core.Features.Events.Commands.DeleteEvent;
using CleanArchitecture.Core.Features.Events.Commands.UpdateEvent;
using CleanArchitecture.Core.Features.Events.Queries.GetAllEvents;
using CleanArchitecture.Core.Features.Events.Queries.GetEventById;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
    }
}
