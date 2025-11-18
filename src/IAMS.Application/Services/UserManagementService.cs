using IAMS.Application.DTOs.Identity;
using IAMS.Application.Interfaces;
using IAMS.Application.Models;
using IAMS.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IAMS.Application.Services
{
    public class UserManagementService : IUserManagementService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly ILogger<UserManagementService> _logger;

        public UserManagementService(
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            ILogger<UserManagementService> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        public async Task<Result<List<UserDto>>> GetAllUsersAsync()
        {
            try
            {
                var users = await _userManager.Users
                    .OrderBy(u => u.FirstName)
                    .ThenBy(u => u.LastName)
                    .ToListAsync();

                var userDtos = new List<UserDto>();

                foreach (var user in users)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    userDtos.Add(new UserDto
                    {
                        Id = user.Id,
                        Email = user.Email ?? string.Empty,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        IsActive = user.IsActive,
                        CreatedOn = user.CreatedOn,
                        LastLogin = user.LastLogin,
                        Roles = roles.ToList()
                    });
                }

                return Result<List<UserDto>>.Success(userDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving users");
                return Result<List<UserDto>>.InternalError("Error retrieving users", new List<string> { ex.Message });
            }
        }

        public async Task<Result<UserDto>> GetUserByIdAsync(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return Result<UserDto>.NotFound("User not found");
                }

                var roles = await _userManager.GetRolesAsync(user);

                var userDto = new UserDto
                {
                    Id = user.Id,
                    Email = user.Email ?? string.Empty,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    IsActive = user.IsActive,
                    CreatedOn = user.CreatedOn,
                    LastLogin = user.LastLogin,
                    Roles = roles.ToList()
                };

                return Result<UserDto>.Success(userDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user {UserId}", userId);
                return Result<UserDto>.InternalError("Error retrieving user", new List<string> { ex.Message });
            }
        }

        public async Task<Result<UserDto>> CreateUserAsync(CreateUserDto dto)
        {
            try
            {
                // Validate password confirmation
                if (dto.Password != dto.ConfirmPassword)
                {
                    return Result<UserDto>.Failure("Passwords do not match");
                }

                // Check if user already exists
                var existingUser = await _userManager.FindByEmailAsync(dto.Email);
                if (existingUser != null)
                {
                    return Result<UserDto>.Failure("User with this email already exists");
                }

                var user = new ApplicationUser
                {
                    UserName = dto.Email,
                    Email = dto.Email,
                    FirstName = dto.FirstName,
                    LastName = dto.LastName,
                    PhoneNumber = dto.PhoneNumber,
                    IsActive = dto.IsActive,
                    CreatedOn = DateTime.UtcNow,
                    EmailConfirmed = true // Auto-confirm for admin-created users
                };

                var result = await _userManager.CreateAsync(user, dto.Password);
                if (!result.Succeeded)
                {
                    return Result<UserDto>.Failure("Failed to create user", result.Errors.Select(e => e.Description).ToList());
                }

                // Assign roles
                if (dto.Roles.Any())
                {
                    var roleResult = await _userManager.AddToRolesAsync(user, dto.Roles);
                    if (!roleResult.Succeeded)
                    {
                        _logger.LogWarning("User created but failed to assign roles: {Errors}",
                            string.Join(", ", roleResult.Errors.Select(e => e.Description)));
                    }
                }

                var userDto = new UserDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    IsActive = user.IsActive,
                    CreatedOn = user.CreatedOn,
                    Roles = dto.Roles
                };

                _logger.LogInformation("User created successfully: {UserId} - {Email}", user.Id, user.Email);
                return Result<UserDto>.Success(userDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user");
                return Result<UserDto>.InternalError("Error creating user", new List<string> { ex.Message });
            }
        }

        public async Task<Result<UserDto>> UpdateUserAsync(UpdateUserDto dto)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(dto.Id);
                if (user == null)
                {
                    return Result<UserDto>.NotFound("User not found");
                }

                // Check if email is being changed and if it's already taken
                if (user.Email != dto.Email)
                {
                    var existingUser = await _userManager.FindByEmailAsync(dto.Email);
                    if (existingUser != null && existingUser.Id != dto.Id)
                    {
                        return Result<UserDto>.Failure("Email is already taken by another user");
                    }
                    user.Email = dto.Email;
                    user.UserName = dto.Email;
                }

                user.FirstName = dto.FirstName;
                user.LastName = dto.LastName;
                user.PhoneNumber = dto.PhoneNumber;
                user.IsActive = dto.IsActive;

                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    return Result<UserDto>.Failure("Failed to update user", result.Errors.Select(e => e.Description).ToList());
                }

                // Update roles
                var currentRoles = await _userManager.GetRolesAsync(user);
                var rolesToRemove = currentRoles.Except(dto.Roles).ToList();
                var rolesToAdd = dto.Roles.Except(currentRoles).ToList();

                if (rolesToRemove.Any())
                {
                    await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
                }

                if (rolesToAdd.Any())
                {
                    await _userManager.AddToRolesAsync(user, rolesToAdd);
                }

                var userDto = new UserDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    IsActive = user.IsActive,
                    CreatedOn = user.CreatedOn,
                    LastLogin = user.LastLogin,
                    Roles = dto.Roles
                };

                _logger.LogInformation("User updated successfully: {UserId}", user.Id);
                return Result<UserDto>.Success(userDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user {UserId}", dto.Id);
                return Result<UserDto>.InternalError("Error updating user", new List<string> { ex.Message });
            }
        }

        public async Task<Result> DeleteUserAsync(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return Result.NotFound("User not found");
                }

                var result = await _userManager.DeleteAsync(user);
                if (!result.Succeeded)
                {
                    return Result.Failure("Failed to delete user", result.Errors.Select(e => e.Description).ToList());
                }

                _logger.LogInformation("User deleted successfully: {UserId}", userId);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user {UserId}", userId);
                return Result.InternalError("Error deleting user", new List<string> { ex.Message });
            }
        }

        public async Task<Result> ChangePasswordAsync(string userId, string newPassword)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return Result.NotFound("User not found");
                }

                // For admin password changes, remove old password and add new one
                // This works better than password reset tokens for admin-initiated changes
                var removeResult = await _userManager.RemovePasswordAsync(user);
                if (!removeResult.Succeeded)
                {
                    _logger.LogWarning("Failed to remove old password for user {UserId}: {Errors}",
                        userId, string.Join(", ", removeResult.Errors.Select(e => e.Description)));
                    return Result.Failure("Failed to remove old password", removeResult.Errors.Select(e => e.Description).ToList());
                }

                var addResult = await _userManager.AddPasswordAsync(user, newPassword);
                if (!addResult.Succeeded)
                {
                    _logger.LogWarning("Failed to add new password for user {UserId}: {Errors}",
                        userId, string.Join(", ", addResult.Errors.Select(e => e.Description)));
                    return Result.Failure("Failed to set new password", addResult.Errors.Select(e => e.Description).ToList());
                }

                _logger.LogInformation("Password changed successfully for user: {UserId}", userId);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password for user {UserId}", userId);
                return Result.InternalError("Error changing password", new List<string> { ex.Message });
            }
        }

        public async Task<Result> ToggleUserStatusAsync(string userId, bool isActive)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return Result.NotFound("User not found");
                }

                user.IsActive = isActive;
                var result = await _userManager.UpdateAsync(user);

                if (!result.Succeeded)
                {
                    return Result.Failure("Failed to update user status", result.Errors.Select(e => e.Description).ToList());
                }

                _logger.LogInformation("User status toggled: {UserId} - IsActive: {IsActive}", userId, isActive);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling user status {UserId}", userId);
                return Result.InternalError("Error updating user status", new List<string> { ex.Message });
            }
        }

        public async Task<Result<List<string>>> GetAllRolesAsync()
        {
            try
            {
                var roles = await _roleManager.Roles
                    .OrderBy(r => r.Name)
                    .Select(r => r.Name!)
                    .ToListAsync();

                return Result<List<string>>.Success(roles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving roles");
                return Result<List<string>>.InternalError("Error retrieving roles", new List<string> { ex.Message });
            }
        }

        public async Task<Result<List<string>>> GetUserRolesAsync(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return Result<List<string>>.NotFound("User not found");
                }

                var roles = await _userManager.GetRolesAsync(user);
                return Result<List<string>>.Success(roles.ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user roles for {UserId}", userId);
                return Result<List<string>>.InternalError("Error retrieving user roles", new List<string> { ex.Message });
            }
        }

        public async Task<Result> UpdateUserRolesAsync(string userId, List<string> roles)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return Result.NotFound("User not found");
                }

                var currentRoles = await _userManager.GetRolesAsync(user);
                var rolesToRemove = currentRoles.Except(roles).ToList();
                var rolesToAdd = roles.Except(currentRoles).ToList();

                if (rolesToRemove.Any())
                {
                    var removeResult = await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
                    if (!removeResult.Succeeded)
                    {
                        return Result.Failure("Failed to remove roles", removeResult.Errors.Select(e => e.Description).ToList());
                    }
                }

                if (rolesToAdd.Any())
                {
                    var addResult = await _userManager.AddToRolesAsync(user, rolesToAdd);
                    if (!addResult.Succeeded)
                    {
                        return Result.Failure("Failed to add roles", addResult.Errors.Select(e => e.Description).ToList());
                    }
                }

                _logger.LogInformation("User roles updated successfully: {UserId}", userId);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user roles for {UserId}", userId);
                return Result.InternalError("Error updating user roles", new List<string> { ex.Message });
            }
        }
    }
}
