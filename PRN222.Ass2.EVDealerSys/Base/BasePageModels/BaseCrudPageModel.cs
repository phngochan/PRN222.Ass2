using PRN222.Ass2.EVDealerSys.BLL.Interfaces;

namespace PRN222.Ass2.EVDealerSys.Base.BasePageModels;

public abstract class BaseCrudPageModel : BasePageModel
{
    protected readonly IActivityLogService _logService;

    protected BaseCrudPageModel(IActivityLogService logService)
    {
        _logService = logService;
    }

    protected int? CurrentUserId
    {
        get
        {
            var idClaim = User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(idClaim, out var id) ? id : null;
        }
    }

    protected string? CurrentUserName
    {
        get
        {
            return User?.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;
        }
    }

    protected async Task LogAsync(string action, string? description = null)
    {
        await _logService.LogAsync(CurrentUserId, CurrentUserName, action, description);
    }
}
