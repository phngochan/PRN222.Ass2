using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PRN222.Ass2.EVDealerSys.Base.BasePageModels;

public abstract class BaseDashboardPageModel<TViewModel> : PageModel
{
    public TViewModel ViewModel { get; protected set; } = default!;

    public abstract Task OnGetAsync();
}
