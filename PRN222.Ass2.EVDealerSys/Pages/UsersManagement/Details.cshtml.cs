using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

using PRN222.Ass2.EVDealerSys.Base.BasePageModels;
using PRN222.Ass2.EVDealerSys.BLL.Interfaces;
using PRN222.Ass2.EVDealerSys.Hubs;
using PRN222.Ass2.EVDealerSys.Models;

namespace PRN222.Ass2.EVDealerSys.Pages.UsersManagement;

[Authorize(Roles = "1,2")]
public class DetailsModel : BaseCrudPageModel
{
    private readonly IUserService _userService;

    public DetailsModel(
        IUserService userService,
        IActivityLogService activityLogService,
        IHubContext<ActivityLogHub> activityLogHubContext)
        : base(activityLogService)
    {
        _userService = userService;
        SetActivityLogHubContext(activityLogHubContext);
    }

    public UserItemViewModel ViewModel { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var user = await _userService.GetUserByIdAsync(id);
        if (user == null)
            return NotFound();

        ViewModel = new UserItemViewModel
        {
            Id = user.Id,
            Name = user.Name ?? "",
            Email = user.Email ?? "",
            Phone = user.Phone,
            Role = user.Role ?? 0,
            RoleName = _userService.GetRoleName(user.Role),
            DealerId = user.DealerId,
            DealerName = user.Dealer?.Name
        };
        await LogAsync("View User Details", $"Viewed details for user: {user.Name} (ID={user.Id})");
        return Page();
    }
}
