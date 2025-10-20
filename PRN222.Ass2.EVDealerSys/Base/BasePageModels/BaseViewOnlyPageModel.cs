namespace PRN222.Ass2.EVDealerSys.Base.BasePageModels;

public abstract class BaseViewOnlyPageModel<TViewModel> : BasePageModel
{
    public TViewModel ViewModel { get; protected set; } = default!;

    public abstract Task OnGetAsync();
}
