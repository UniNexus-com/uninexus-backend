using CleanArchitecture.Core.Features.Inventory.Commands.CreateInventoryItem;
using CleanArchitecture.Core.Features.Inventory.Commands.DeleteInventoryItem;
using CleanArchitecture.Core.Features.Inventory.Commands.UpdateInventoryItemStatus;
using CleanArchitecture.Core.Features.Inventory.Commands.BorrowAsset;
using CleanArchitecture.Core.Features.Inventory.Commands.ReturnAsset;
using CleanArchitecture.Core.Features.Inventory.Queries.GetInventory;
using CleanArchitecture.Core.Features.Inventory.Queries.GetMyBorrowedAssets;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CleanArchitecture.WebApi.Controllers
{
    [Authorize]
    public class AssetsController : BaseApiController
    {
        [HttpGet]
        public async Task<IActionResult> Get(
            [FromQuery] int? clubId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string searchValue = "",
            [FromQuery] string sortColumn = "Name",
            [FromQuery] string sortDirection = "asc",
            [FromQuery] List<string> categoryFilters = null,
            [FromQuery] List<string> conditionFilters = null,
            [FromQuery] List<string> statusFilters = null)
            => Ok(await Mediator.Send(new GetInventoryQuery
            {
                ClubId = clubId,
                PageNumber = pageNumber,
                PageSize = pageSize,
                SearchValue = searchValue,
                SortColumn = sortColumn,
                SortDirection = sortDirection,
                CategoryFilters = categoryFilters,
                ConditionFilters = conditionFilters,
                StatusFilters = statusFilters
            }));

        [HttpGet("my-borrowed")]
        public async Task<IActionResult> GetMyBorrowed()
            => Ok(await Mediator.Send(new GetMyBorrowedAssetsQuery()));

        [HttpPost]
        public async Task<IActionResult> Post(CreateInventoryItemCommand command)
            => Ok(await Mediator.Send(command));

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, UpdateInventoryItemStatusCommand command)
        {
            if (id != command.Id) return BadRequest();
            return Ok(await Mediator.Send(command));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
            => Ok(await Mediator.Send(new DeleteInventoryItemCommand { Id = id }));

        /// <summary>POST /v1/Assets/{id}/borrow — QR ile ekipman ödünç al</summary>
        [HttpPost("{id}/borrow")]
        public async Task<IActionResult> Borrow(int id, [FromBody] BorrowAssetCommand command)
        {
            command.AssetId = id;
            return Ok(await Mediator.Send(command));
        }

        /// <summary>POST /v1/Assets/{id}/return — QR ile ekipman iade et</summary>
        [HttpPost("{id}/return")]
        public async Task<IActionResult> Return(int id)
            => Ok(await Mediator.Send(new ReturnAssetCommand { AssetId = id }));
    }
}
