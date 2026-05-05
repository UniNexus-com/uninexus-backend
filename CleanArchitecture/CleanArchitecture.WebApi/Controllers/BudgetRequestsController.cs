using CleanArchitecture.Core.Features.Finance.Commands.CreateBudgetRequest;
using CleanArchitecture.Core.Features.Finance.Commands.DeleteBudgetRequest;
using CleanArchitecture.Core.Features.Finance.Commands.UpdateBudgetRequestStatus;
using CleanArchitecture.Core.Features.Finance.Queries.GetFinanceSummary;
using CleanArchitecture.Core.Features.Finance.Queries.GetPagedBudgetRequests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CleanArchitecture.WebApi.Controllers
{
    [Authorize]
    public class BudgetRequestsController : BaseApiController
    {
        private readonly IWebHostEnvironment _env;

        public BudgetRequestsController(IWebHostEnvironment env)
        {
            _env = env;
        }

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

        private string GetUploadDir(int id)
        {
            var root = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
            return Path.Combine(root, "uploads", "budget-requests", id.ToString());
        }

        [HttpPost("{id}/files")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadFiles(int id, [FromForm] List<IFormFile> files)
        {
            if (files == null || files.Count == 0)
                return BadRequest(new { message = "No files provided." });

            var uploadDir = GetUploadDir(id);
            Directory.CreateDirectory(uploadDir);

            var uploaded = new List<object>();
            foreach (var file in files)
            {
                if (file.Length == 0) continue;
                var safeName = Path.GetFileName(file.FileName);
                var filePath = Path.Combine(uploadDir, safeName);
                using var stream = new FileStream(filePath, FileMode.Create);
                await file.CopyToAsync(stream);
                uploaded.Add(new
                {
                    name = safeName,
                    size = file.Length,
                    url = $"/uploads/budget-requests/{id}/{safeName}"
                });
            }
            return Ok(new { data = uploaded });
        }

        [HttpGet("{id}/files")]
        public IActionResult GetFiles(int id)
        {
            var uploadDir = GetUploadDir(id);
            if (!Directory.Exists(uploadDir))
                return Ok(new { data = new List<object>() });

            var files = Directory.GetFiles(uploadDir)
                .Select(f => new
                {
                    name = Path.GetFileName(f),
                    size = new FileInfo(f).Length,
                    url = $"/uploads/budget-requests/{id}/{Path.GetFileName(f)}"
                })
                .ToList();
            return Ok(new { data = files });
        }
    }
}
