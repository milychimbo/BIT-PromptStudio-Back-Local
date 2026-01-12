using BitPromptStudioBackend.Models;
using BitPromptStudioBackend.Services;
using Microsoft.AspNetCore.Mvc;

namespace BitPromptStudioBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AgentController : ControllerBase
    {
        private readonly IAgenteService _agenteService;

        public AgentController(IAgenteService agenteService)
        {
            _agenteService = agenteService;
        }

        /// <summary>
        /// Corrige y mejora un prompt usando AI Foundry Agent
        /// </summary>
        [HttpPost("fix-prompt")]
        public async Task<IActionResult> FixPrompt(
            [FromBody] FixPromptRequest request,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Prompt))
                return BadRequest("El prompt no puede estar vacío.");

            var result = await _agenteService.FixPromptAsync(
                request.Prompt,
                cancellationToken
            );

            return Ok(result);
        }
    }
}
