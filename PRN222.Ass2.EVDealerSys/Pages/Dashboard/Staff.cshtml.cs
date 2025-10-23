using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

using PRN222.Ass2.EVDealerSys.Base.BasePageModels;
using PRN222.Ass2.EVDealerSys.BLL.Interfaces;
using PRN222.Ass2.EVDealerSys.Hubs;
using PRN222.Ass2.EVDealerSys.Models.Dashboard;

namespace PRN222.Ass2.EVDealerSys.Pages.Dashboard
{
    [Authorize(Roles = "3")]
    public class StaffModel : BaseCrudPageModel
    {
        private readonly IOrderService _orderService;

        public StaffDashboardViewModel ViewModel { get; set; } = new();

        public StaffModel(
            IActivityLogService logService,
            IOrderService orderService,
            IHubContext<ActivityLogHub> activityLogHubContext) : base(logService)
        {
            _orderService = orderService;
            SetActivityLogHubContext(activityLogHubContext);
        }

        public async Task OnGetAsync()
        {
            var userId = CurrentUserId ?? 0;
            
            ViewModel = new StaffDashboardViewModel
            {
                UserName = User.Identity?.Name ?? "Staff",
                MyOrdersToday = await _orderService.GetOrdersCountByUserAndDateRangeAsync(
                    userId, DateTime.Today, DateTime.Today.AddDays(1)
                ),
            };

            await LogAsync("View Staff Dashboard", "Staff accessed dashboard");
        }
    }
}
