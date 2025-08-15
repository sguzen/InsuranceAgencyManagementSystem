
using IAMS.Domain.Entities;

namespace IAMS.Identity.Services
{
    public interface ITokenService
    {
        RefreshToken GenerateRefreshToken();
        string GenerateJwtToken(ApplicationUser user, IList<string> roles);

    }
}