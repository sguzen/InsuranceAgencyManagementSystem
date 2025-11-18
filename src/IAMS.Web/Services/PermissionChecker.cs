using Microsoft.AspNetCore.Components.Authorization;

namespace IAMS.Web.Services
{
    public class PermissionChecker : IPermissionChecker
    {
        private readonly AuthenticationStateProvider _authenticationStateProvider;

        public PermissionChecker(AuthenticationStateProvider authenticationStateProvider)
        {
            _authenticationStateProvider = authenticationStateProvider;
        }

        public async Task<bool> HasPermissionAsync(string permission)
        {
            var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;

            if (!user.Identity?.IsAuthenticated ?? true)
                return false;

            return user.HasClaim("permission", permission);
        }

        public async Task<bool> HasAnyPermissionAsync(params string[] permissions)
        {
            var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;

            if (!user.Identity?.IsAuthenticated ?? true)
                return false;

            return permissions.Any(permission => user.HasClaim("permission", permission));
        }

        public async Task<bool> HasAllPermissionsAsync(params string[] permissions)
        {
            var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;

            if (!user.Identity?.IsAuthenticated ?? true)
                return false;

            return permissions.All(permission => user.HasClaim("permission", permission));
        }
    }
}
