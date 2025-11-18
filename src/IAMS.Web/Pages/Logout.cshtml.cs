using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using IAMS.Domain.Entities;

namespace IAMS.Web.Pages
{
    public class LogoutModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<LogoutModel> _logger;

        public LogoutModel(
            SignInManager<ApplicationUser> signInManager,
            ILogger<LogoutModel> logger)
        {
            _signInManager = signInManager;
            _logger = logger;
        }

        public async Task<IActionResult> OnGet()
        {
            return await PerformLogout();
        }

        public async Task<IActionResult> OnPost()
        {
            return await PerformLogout();
        }

        private async Task<IActionResult> PerformLogout()
        {
            try
            {
                var userName = User?.Identity?.Name;

                // Sign out the user
                await _signInManager.SignOutAsync();

                _logger.LogInformation("User {UserName} logged out successfully", userName ?? "Unknown");

                // Redirect to login page
                return Redirect("/login");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
                // Still redirect to login even if there's an error
                return Redirect("/login");
            }
        }
    }
}
