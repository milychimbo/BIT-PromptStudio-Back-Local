using BitPromptStudioBackend.DTOs;
using BitPromptStudioBackend.Services.interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BitPromptStudioBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PromptsController : ControllerBase
    {
        private readonly IPromptService _promptService;

        public PromptsController(IPromptService promptService)
        {
            _promptService = promptService;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreatePromptDto dto)
        {
            try
            {
                var result = await _promptService.CreatePromptAsync(dto);
                return CreatedAtAction(nameof(GetDetail), new { id = result.Id }, result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message }); // 400 for logic errors (like score < 80)
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred creating the prompt.", details = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetFeed([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var result = await _promptService.GetFeedAsync(page, pageSize);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDetail(Guid id)
        {
            var result = await _promptService.GetPromptDetailAsync(id);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpPost("{id}/versions")]
        public async Task<IActionResult> AddVersion(Guid id, [FromBody] CreateVersionDto dto)
        {
            try
            {
                var result = await _promptService.AddVersionAsync(id, dto);
                return Ok(result);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred updating the prompt.", details = ex.Message });
            }
        }

        [HttpGet("{id}/versions")]
        public async Task<IActionResult> GetVersions(Guid id)
        {
            var result = await _promptService.GetPromptVersionsAsync(id);
            return Ok(result);
        }

        [HttpGet("versions/{versionId}")]
        public async Task<IActionResult> GetVersionDetail(Guid versionId)
        {
            var result = await _promptService.GetVersionDetailAsync(versionId);
            if (result == null) return NotFound();
            return Ok(result);
        }
    }
}
