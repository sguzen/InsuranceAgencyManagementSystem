using IAMS.Identity.Models;

namespace IAMS.Identity.Services
{
    public interface ITokenService
    {
        RefreshToken GenerateRefreshToken();
        string GenerateJwtToken(ApplicationUser user, IList<string> roles);

    }
}