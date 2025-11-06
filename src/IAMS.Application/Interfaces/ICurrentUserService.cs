using System.Security.Claims;

namespace IAMS.Application.Interfaces
{
    public interface ICurrentUserService
    {
        string? UserId { get; }
        string? UserName { get; }
        string? Email { get; }
        string? FirstName { get; }
        string? LastName { get; }
         
        bool IsAuthenticated { get; }
        List<string> Roles { get; }
        List<string> Permissions { get; }

        ClaimsPrincipal? GetClaimsPrincipal();
        bool HasPermission(string permission);
        bool HasRole(string role);
        bool HasAnyRole(params string[] roles);
        bool HasAnyPermission(params string[] permissions);
    }
}