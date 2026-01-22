using System;

namespace BitPromptStudioBackend.DTOs
{
    public class UserDto
    {
        public Guid Id { get; set; }
        public string EntraObjectId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        
        public RoleDto? Role { get; set; }
        
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateUserDto
    {
        public string EntraObjectId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        // RoleId is optional here because we might assign default 'Viewer'
    }

    public class UpdateUserRoleDto
    {
        public Guid RoleId { get; set; }
    }

    public class RoleDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}
