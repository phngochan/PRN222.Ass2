using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

using PRN222.Ass2.EVDealerSys.Base.BasePageModels;
using PRN222.Ass2.EVDealerSys.BLL.Interfaces;
using PRN222.Ass2.EVDealerSys.Hubs;
using PRN222.Ass2.EVDealerSys.Models.Dashboard;

namespace PRN222.Ass2.EVDealerSys.Pages.Dashboard
{
    [Authorize(Roles = "2")]
    public class ManagerModel : BaseCrudPageModel
    {
        private readonly IUserService _userService;
        private readonly IOrderService _orderService;
        private readonly ICustomerService _customerService;

        public ManagerDashboardViewModel ViewModel { get; set; } = new();

        public ManagerModel(
            IActivityLogService logService,
            IUserService userService,
            IOrderService orderService,
            ICustomerService customerService,
            IHubContext<ActivityLogHub> activityLogHubContext) : base(logService)
        {
            _userService = userService;
            _orderService = orderService;
            _customerService = customerService;
            SetActivityLogHubContext(activityLogHubContext);
        }

        public async Task OnGetAsync()
        {
            ViewModel = new ManagerDashboardViewModel
            {
                UserName = User.Identity?.Name ?? "Manager",
                StaffCount = (await _userService.GetAllUsersAsync()).Count(u => u.Role == 3),
                OrdersThisMonth = await _orderService.GetOrdersCountByDateRangeAsync(
                    new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1),
                    DateTime.Now
                ),
                CustomersThisMonth = await _customerService.GetCustomersCountByDateRangeAsync(
                    new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1),
                    DateTime.Now
                ),
            };

            await LogAsync("View Manager Dashboard", "Manager accessed dashboard");
        }
    }
}
