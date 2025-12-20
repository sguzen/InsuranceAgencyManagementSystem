using Microsoft.AspNetCore.Authentication;

namespace IAMS.Api.Authentication
{
    public static class ApiKeyAuthenticationExtensions
    {
        public static AuthenticationBuilder AddApiKeyAuthentication(
            this AuthenticationBuilder builder,
            string authenticationScheme,
            Action<ApiKeyAuthenticationOptions> configureOptions)
        {
            return builder.AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(
                authenticationScheme,
                configureOptions);
        }

        public static AuthenticationBuilder AddApiKeyAuthentication(
            this AuthenticationBuilder builder,
            Action<ApiKeyAuthenticationOptions> configureOptions)
        {
            return builder.AddApiKeyAuthentication(
                ApiKeyAuthenticationOptions.DefaultScheme,
                configureOptions);
        }
    }
}
