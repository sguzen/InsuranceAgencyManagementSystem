using Microsoft.AspNetCore.Authentication;

namespace IAMS.Api.Authentication
{
    public class ApiKeyAuthenticationOptions : AuthenticationSchemeOptions
    {
        public const string DefaultScheme = "ApiKey";
        public string ApiKey { get; set; } = string.Empty;
    }
}
