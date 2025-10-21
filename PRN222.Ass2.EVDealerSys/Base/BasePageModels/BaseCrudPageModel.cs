using Microsoft.AspNetCore.SignalR;

using PRN222.Ass2.EVDealerSys.BLL.Interfaces;
using PRN222.Ass2.EVDealerSys.Hubs;

namespace PRN222.Ass2.EVDealerSys.Base.BasePageModels;

public abstract class BaseCrudPageModel : BasePageModel
{
    protected readonly IActivityLogService _logService;
    protected IHubContext<ActivityLogHub>? ActivityLogHubContext { get; private set; }

    protected BaseCrudPageModel(IActivityLogService logService)
    {
        _logService = logService;
    }

    public void SetActivityLogHubContext(IHubContext<ActivityLogHub> hubContext)
    {
        ActivityLogHubContext = hubContext;
    }

    protected async Task NotifyActivityLogAsync(string action, string? description = null)
    {
        if (ActivityLogHubContext != null)
        {
            await ActivityLogHubContext.Clients.All.SendAsync("ReceiveNewLog", new
            {
                action,
                description,
                userId = CurrentUserId,
                userName = CurrentUserName ?? "N/A",
                createdAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            });
        }
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
        
        // Automatically send SignalR notification after logging
        await NotifyActivityLogAsync(action, description);
    }
}
