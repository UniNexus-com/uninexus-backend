using CleanArchitecture.Core.Features.Analytics.Queries.GetCampusHeatmap;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace CleanArchitecture.WebApi.Controllers
{
    [Authorize(Roles = "SKS_ADMIN")]
    public class CampusHeatmapController : BaseApiController
    {
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            return Ok(await Mediator.Send(new GetCampusHeatmapQuery()));
        }
    }
}
