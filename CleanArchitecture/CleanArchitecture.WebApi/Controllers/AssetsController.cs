using CleanArchitecture.Core.Features.Inventory.Commands.CreateInventoryItem;
using CleanArchitecture.Core.Features.Inventory.Commands.DeleteInventoryItem;
using CleanArchitecture.Core.Features.Inventory.Commands.UpdateInventoryItemStatus;
using CleanArchitecture.Core.Features.Inventory.Commands.BorrowAsset;
using CleanArchitecture.Core.Features.Inventory.Commands.ReturnAsset;
using CleanArchitecture.Core.Features.Inventory.Queries.GetInventory;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace CleanArchitecture.WebApi.Controllers
{
    [Authorize]
    public class AssetsController : BaseApiController
    {
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] int? clubId)
            => Ok(await Mediator.Send(new GetInventoryQuery { ClubId = clubId }));

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
        public async Task<IActionResult> Borrow(int id)
            => Ok(await Mediator.Send(new BorrowAssetCommand { AssetId = id }));

        /// <summary>POST /v1/Assets/{id}/return — QR ile ekipman iade et</summary>
        [HttpPost("{id}/return")]
        public async Task<IActionResult> Return(int id)
            => Ok(await Mediator.Send(new ReturnAssetCommand { AssetId = id }));
    }
}
