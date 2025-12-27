using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IAMS.Web.Pages
{
    public class LogoutModel : PageModel
    {
        public IActionResult OnGet()
        {
            // Redirect to the Blazor logout page for better UX
            return Redirect("/account/logout");
        }

        public IActionResult OnPost()
        {
            // Redirect to the Blazor logout page for better UX
            return Redirect("/account/logout");
        }
    }
}
