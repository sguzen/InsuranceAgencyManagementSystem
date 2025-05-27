using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IAMS.Application.DTOs.Identity
{
    public class LoginDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class AuthResponseDto
    {
        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string JwtToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new();
        public List<string> Permissions { get; set; } = new();
    }

    public class RefreshTokenDto
    {
        public string RefreshToken { get; set; } = string.Empty;
    }

    public class RegisterUserDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? RoleName { get; set; }
    }

    public class UserDto
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? LastLogin { get; set; }
        public List<string> Roles { get; set; } = new();
    }

    public class UserDetailsDto : UserDto
    {
        // Additional details can be added here
    }

    public class CreateUserDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? RoleName { get; set; }
    }

    public class UpdateUserDto
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public List<string>? Roles { get; set; }
    }

    public class ChangePasswordDto
    {
        public string NewPassword { get; set; } = string.Empty;
    }

    public class ChangeMyPasswordDto
    {
        public string CurrentPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }

    public class RoleDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsDefault { get; set; }
        public bool IsSystem { get; set; }
    }

    public class CreateRoleDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsDefault { get; set; }
        public List<int>? PermissionIds { get; set; }
    }

    public class UpdateRoleDto
    {
        public string? Description { get; set; }
        public bool IsDefault { get; set; }
    }

    public class PermissionDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Module { get; set; } = string.Empty;
    }

    public class UpdateRolePermissionsDto
    {
        public List<int> PermissionIds { get; set; } = new();
    }
}
