using CleanArchitecture.Core.Features.Finance.Commands.CreateBudgetRequest;
using CleanArchitecture.Core.Features.Finance.Commands.DeleteBudgetRequest;
using CleanArchitecture.Core.Features.Finance.Commands.UpdateBudgetRequestStatus;
using CleanArchitecture.Core.Features.Finance.Queries.GetFinanceSummary;
using CleanArchitecture.Core.Features.Finance.Queries.GetPagedBudgetRequests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace CleanArchitecture.WebApi.Controllers
{
    [Authorize]
    public class BudgetRequestsController : BaseApiController
    {
        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary([FromQuery] int? clubId)
            => Ok(await Mediator.Send(new GetFinanceSummaryQuery { ClubId = clubId }));

        [HttpGet]
        public async Task<IActionResult> GetPaged([FromQuery] GetPagedBudgetRequestsQuery filter)
            => Ok(await Mediator.Send(filter));

        [HttpPost]
        public async Task<IActionResult> Post(CreateBudgetRequestCommand command)
            => Ok(await Mediator.Send(command));

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, UpdateBudgetRequestStatusCommand command)
        {
            if (id != command.Id) return BadRequest();
            return Ok(await Mediator.Send(command));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
            => Ok(await Mediator.Send(new DeleteBudgetRequestCommand { Id = id }));
    }
}
