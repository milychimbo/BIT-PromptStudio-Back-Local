using BitPromptStudioBackend.Context;
using BitPromptStudioBackend.DTOs;
using BitPromptStudioBackend.Entities;
using BitPromptStudioBackend.Services.interfaces;
using Microsoft.EntityFrameworkCore;

namespace BitPromptStudioBackend.Services
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;

        public UserService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<UserDto>> GetAllUsersAsync()
        {
            var users = await _context.Users
                .Include(u => u.Role)
                .AsNoTracking()
                .ToListAsync();

            return users.Select(MapToDto).ToList();
        }

        public async Task<UserDto?> GetUserByIdAsync(Guid id)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null) return null;
            return MapToDto(user);
        }

        public async Task<UserDto> CreateOrUpdateUserAsync(CreateUserDto dto)
        {
            // Check if user exists by EntraObjectId
            var existingUser = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.EntraObjectId == dto.EntraObjectId);

            if (existingUser != null)
            {
                // Update basic info if changed (optional)
                bool changed = false;
                if (existingUser.Email != dto.Email)
                {
                    existingUser.Email = dto.Email;
                    changed = true;
                }
                if (existingUser.FullName != dto.FullName)
                {
                    existingUser.FullName = dto.FullName;
                    changed = true;
                }

                if (changed)
                {
                    await _context.SaveChangesAsync();
                }

                return MapToDto(existingUser);
            }

            // Create new user
            // Default role: Viewer (need to find the Viewer role ID)
            var viewerRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Viewer");
            if (viewerRole == null)
            {
                // Fallback or error. Assuming Seed ran, it should exist. 
                // Creating one if strict consistency needed, or throw.
                throw new InvalidOperationException("Default role 'Viewer' not found.");
            }

            var newUser = new User
            {
                Id = Guid.NewGuid(),
                EntraObjectId = dto.EntraObjectId,
                Email = dto.Email,
                FullName = dto.FullName,
                RoleId = viewerRole.Id,
                Role = viewerRole,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            return MapToDto(newUser);
        }

        public async Task<UserDto> UpdateUserRoleAsync(Guid userId, Guid roleId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) throw new KeyNotFoundException("No se encontró el usuario");

            var role = await _context.Roles.FindAsync(roleId);
            if (role == null) throw new KeyNotFoundException("No se encontró el rol");

            user.RoleId = roleId;
            await _context.SaveChangesAsync();
            
            // Reload with role to return full DTO
            await _context.Entry(user).Reference(u => u.Role).LoadAsync();

            return MapToDto(user);
        }

        public async Task<List<RoleDto>> GetAllRolesAsync()
        {
            var roles = await _context.Roles.AsNoTracking().ToListAsync();
            return roles.Select(r => new RoleDto
            {
                Id = r.Id,
                Name = r.Name,
                Description = r.Description ?? string.Empty
            }).ToList();
        }

        private static UserDto MapToDto(User u)
        {
            return new UserDto
            {
                Id = u.Id,
                EntraObjectId = u.EntraObjectId,
                Email = u.Email,
                FullName = u.FullName,
                CreatedAt = u.CreatedAt,
                IsActive = u.IsActive,
                Role = u.Role == null ? null : new RoleDto
                {
                    Id = u.Role.Id,
                    Name = u.Role.Name,
                    Description = u.Role.Description ?? string.Empty
                }
            };
        }
    }
}
