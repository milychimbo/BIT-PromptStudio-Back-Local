using BitPromptStudioBackend.Services.interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BitPromptStudioBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RolesController : ControllerBase
    {
        private readonly IUserService _userService;

        public RolesController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _userService.GetAllRolesAsync();
            return Ok(result);
        }
    }
}
