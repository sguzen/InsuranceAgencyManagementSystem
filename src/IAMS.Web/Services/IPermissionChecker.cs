namespace IAMS.Web.Services
{
    public interface IPermissionChecker
    {
        Task<bool> HasPermissionAsync(string permission);
        Task<bool> HasAnyPermissionAsync(params string[] permissions);
        Task<bool> HasAllPermissionsAsync(params string[] permissions);
    }
}
