using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PRN222.Ass2.EVDealerSys.Base.BasePageModels;

public class BasePageModel : PageModel
{
    protected void SetSuccess(string message) => TempData["SuccessMessage"] = message;
    protected void SetError(string message) => TempData["ErrorMessage"] = message;
}
