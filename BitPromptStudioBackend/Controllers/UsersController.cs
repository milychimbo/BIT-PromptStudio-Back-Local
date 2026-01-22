using BitPromptStudioBackend.DTOs;
using BitPromptStudioBackend.Services.interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BitPromptStudioBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _userService.GetAllUsersAsync();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _userService.GetUserByIdAsync(id);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpPost("sync")] 
        // Called when user logs in via frontend to ensure they exist in DB
        public async Task<IActionResult> SyncUser([FromBody] CreateUserDto dto)
        {
            try 
            {
                var result = await _userService.CreateOrUpdateUserAsync(dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error syncing user", details = ex.Message });
            }
        }

        [HttpPut("{id}/role")]
        public async Task<IActionResult> UpdateRole(Guid id, [FromBody] UpdateUserRoleDto dto)
        {
            try
            {
                var result = await _userService.UpdateUserRoleAsync(id, dto.RoleId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error updating user role", details = ex.Message });
            }
        }
    }
}
