using BitPromptStudioBackend.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BitPromptStudioBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestDbController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TestDbController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("ping")]
        public IActionResult Ping()
        {
            try
            {
                _context.Database.OpenConnection(); 
                _context.Database.CloseConnection();
                return Ok("Database connection successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Database connection failed: {ex.Message}");
            }
            }
        [HttpPost("run-migration")]
        public async Task<IActionResult> RunMigration([FromQuery] string scriptName = "001_CreatePromptsAndTags.sql")
        {
            try
            {
                // Basic sanitation
                scriptName = Path.GetFileName(scriptName);
                
                var scriptPath = Path.Combine(Directory.GetCurrentDirectory(), "Scripts", scriptName);
                if (!System.IO.File.Exists(scriptPath))
                    return NotFound($"Script file '{scriptName}' not found.");

                var script = await System.IO.File.ReadAllTextAsync(scriptPath);

                // Split by "GO" (case insensitive/multiline)
                var commands = System.Text.RegularExpressions.Regex.Split(
                    script,
                    @"^\s*GO\s*$",
                    System.Text.RegularExpressions.RegexOptions.Multiline | System.Text.RegularExpressions.RegexOptions.IgnoreCase
                );

                using var transaction = _context.Database.BeginTransaction();
                try
                {
                    foreach (var command in commands)
                    {
                        if (string.IsNullOrWhiteSpace(command)) continue;
                        await _context.Database.ExecuteSqlRawAsync(command);
                    }
                    await transaction.CommitAsync();
                    return Ok($"Migration '{scriptName}' executed successfully.");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return StatusCode(500, $"SQL Error: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Migration failed: {ex.Message}");
            }
        }
    }
}
