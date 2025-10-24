using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace PRN222.Ass2.EVDealerSys.Pages.Dashboard
{
    [Authorize]
    public class IndexModel : PageModel
    {
        public IActionResult OnGet()
        {
            var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;

            return roleClaim switch
            {
                "1" => RedirectToPage("/Dashboard/Admin"),
                "2" => RedirectToPage("/Dashboard/Manager"),
                "3" => RedirectToPage("/Dashboard/Staff"),
                _ => RedirectToPage("/Account/Login")
            };
        }
    }
}
