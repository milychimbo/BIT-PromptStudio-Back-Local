using BitPromptStudioBackend.DTOs;

namespace BitPromptStudioBackend.Services.interfaces
{
    public interface IUserService
    {
        Task<List<UserDto>> GetAllUsersAsync();
        Task<UserDto?> GetUserByIdAsync(Guid id);
        Task<UserDto> CreateOrUpdateUserAsync(CreateUserDto dto); // For deterministic sync
        Task<UserDto> UpdateUserRoleAsync(Guid userId, Guid roleId);
        Task<List<RoleDto>> GetAllRolesAsync();
    }
}
