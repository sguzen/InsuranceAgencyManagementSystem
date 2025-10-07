using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using IAMS.Web.Services;

namespace IAMS.Web.Pages
{
    public class LoginModel : PageModel
    {
        private readonly IAuthService _authService;
        private readonly ILogger<LoginModel> _logger;

        public LoginModel(IAuthService authService, ILogger<LoginModel> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [BindProperty]
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = string.Empty;

        [BindProperty]
        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [BindProperty]
        public bool RememberMe { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? ReturnUrl { get; set; }

        public string? ErrorMessage { get; set; }

        public void OnGet()
        {
            // Just display the login form
        }

        public async Task<IActionResult> OnPostAsync()
        {
            _logger.LogInformation("Posted Email: '{Email}', Password length: {Length}", Email, Password?.Length ?? 0);

            if (!ModelState.IsValid)
            {
                ErrorMessage = "Please correct the errors below.";
                return Page();
            }

            try
            {
                _logger.LogInformation("Login attempt for {Email}", Email);

                // Call AuthService to verify credentials
                var authResult = await _authService.LoginAsync(Email, Password);

                if (authResult == null)
                {
                    _logger.LogWarning("Login failed for {Email}", Email);
                    ErrorMessage = "Invalid email or password";
                    return Page();
                }

                // Create claims from the auth result
                var claims = new List<Claim>
                {
                    new(ClaimTypes.NameIdentifier, authResult.UserId),
                    new(ClaimTypes.Email, authResult.Email),
                    new(ClaimTypes.GivenName, authResult.FirstName),
                    new(ClaimTypes.Surname, authResult.LastName),
                    new(ClaimTypes.Name, $"{authResult.FirstName} {authResult.LastName}".Trim())
                };

                // Add roles
                foreach (var role in authResult.Roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }

                // Add permissions
                foreach (var permission in authResult.Permissions)
                {
                    claims.Add(new Claim("permission", permission));
                }

                // Create identity and sign in
                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    principal,
                    new AuthenticationProperties
                    {
                        IsPersistent = RememberMe,
                        ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
                    });

                _logger.LogInformation("User {Email} logged in successfully", Email);

                // Redirect to return URL or home
                if (!string.IsNullOrEmpty(ReturnUrl) && Url.IsLocalUrl(ReturnUrl))
                {
                    return Redirect(ReturnUrl);
                }

                return Redirect("/");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for {Email}", Email);
                ErrorMessage = "An error occurred during login. Please try again.";
                return Page();
            }
        }
    }
}