using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using IAMS.Web.Services;
using IAMS.Domain.Entities;

namespace IAMS.Web.Pages
{
    public class LoginModel : PageModel
    {
        private readonly IAuthService _authService;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<LoginModel> _logger;

        public LoginModel(
            IAuthService authService,
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            ILogger<LoginModel> logger)
        {
            _authService = authService;
            _signInManager = signInManager;
            _userManager = userManager;
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

                // Find user by email
                var user = await _userManager.FindByEmailAsync(Email);
                if (user == null || !user.IsActive)
                {
                    _logger.LogWarning("Login failed for {Email} - user not found or inactive", Email);
                    ErrorMessage = "Invalid email or password";
                    return Page();
                }

                // Sign in using Identity's SignInManager
                var result = await _signInManager.PasswordSignInAsync(
                    user.UserName ?? Email,
                    Password,
                    RememberMe,
                    lockoutOnFailure: true);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User {Email} authenticated successfully", Email);

                    // Update last login
                    user.LastLogin = DateTime.UtcNow;
                    await _userManager.UpdateAsync(user);

                    _logger.LogInformation("User {Email} logged in successfully", Email);

                    // Redirect to return URL or home
                    if (!string.IsNullOrEmpty(ReturnUrl) && Url.IsLocalUrl(ReturnUrl))
                    {
                        return Redirect(ReturnUrl);
                    }

                    return Redirect("/");
                }
                else if (result.IsLockedOut)
                {
                    _logger.LogWarning("Account locked out for {Email}", Email);
                    ErrorMessage = "Account is locked out. Please try again later.";
                    return Page();
                }
                else
                {
                    _logger.LogWarning("Login failed for {Email}", Email);
                    ErrorMessage = "Invalid email or password";
                    return Page();
                }
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