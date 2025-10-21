using Microsoft.AspNetCore.Authorization;

using PRN222.Ass2.EVDealerSys.Base.BasePageModels;
using PRN222.Ass2.EVDealerSys.BLL.Interfaces;
using PRN222.Ass2.EVDealerSys.Models.Dashboard;

namespace PRN222.Ass2.EVDealerSys.Pages.Dashboard;

[Authorize(Roles = "1")]
public class AdminModel : BaseViewOnlyPageModel<AdminDashboardViewModel>
{
    private readonly IUserService _userService;
    private readonly IDealerService _dealerService;
    private readonly IOrderService _orderService;
    private readonly IVehicleService _vehicleService;

    public AdminModel(
        IUserService userService,
        IDealerService dealerService,
        IOrderService orderService,
        IVehicleService vehicleService)
    {
        _userService = userService;
        _dealerService = dealerService;
        _orderService = orderService;
        _vehicleService = vehicleService;
    }

    public override async Task OnGetAsync()
    {
        ViewModel = new AdminDashboardViewModel
        {
            UserName = User.Identity?.Name ?? "Admin",
            TotalUsers = await GetTotalUsersCount(),
            TotalDealers = await GetTotalDealersCount(),
            TotalOrders = await GetTotalOrdersCount(),
            TotalVehicles = await GetTotalVehiclesCount(),
        };
    }

    // ========================== PRIVATE HELPERS ==========================

    private async Task<int> GetTotalUsersCount()
    {
        try
        {
            var users = await _userService.GetAllUsersAsync();
            return users.Count();
        }
        catch { return 0; }
    }

    private async Task<int> GetTotalDealersCount()
    {
        try
        {
            var dealers = await _dealerService.GetAllDealersAsync();
            return dealers.Count();
        }
        catch { return 0; }
    }

    private async Task<int> GetTotalOrdersCount()
    {
        try
        {
            var orders = await _orderService.GetAllOrdersAsync();
            return orders.Count();
        }
        catch { return 0; }
    }

    private async Task<int> GetTotalVehiclesCount()
    {
        try
        {
            return await _vehicleService.GetTotalVehiclesCountAsync();
        }
        catch { return 0; }
    }

    private async Task<List<RecentUserViewModel>> GetRecentUsers()
    {
        try
        {
            var users = await _userService.GetAllUsersAsync();
            return users
                .OrderByDescending(u => u.Id)
                .Take(5)
                .Select(u => new RecentUserViewModel
                {
                    Id = u.Id,
                    Name = u.Name ?? "",
                    Email = u.Email ?? "",
                    Role = u.Role ?? 0,
                    RoleName = _userService.GetRoleName(u.Role),
                    CreatedDate = DateTime.Now.AddDays(-new Random().Next(1, 30))
                }).ToList();
        }
        catch { return new List<RecentUserViewModel>(); }
    }

    private async Task<SystemStatsViewModel> GetSystemStats()
    {
        try
        {
            var currentMonth = DateTime.Now;
            var lastMonth = currentMonth.AddMonths(-1);

            var currentOrdersCount = await _orderService.GetOrdersCountByDateRangeAsync(
                new DateTime(currentMonth.Year, currentMonth.Month, 1),
                currentMonth);

            var lastMonthOrdersCount = await _orderService.GetOrdersCountByDateRangeAsync(
                new DateTime(lastMonth.Year, lastMonth.Month, 1),
                new DateTime(lastMonth.Year, lastMonth.Month, DateTime.DaysInMonth(lastMonth.Year, lastMonth.Month)));

            var ordersGrowth = lastMonthOrdersCount > 0
                ? $"{((double)(currentOrdersCount - lastMonthOrdersCount) / lastMonthOrdersCount * 100):F1}%"
                : "+0%";

            return new SystemStatsViewModel
            {
                UsersGrowth = "+12%",
                OrdersGrowth = ordersGrowth,
                RevenueGrowth = "+15%",
                CustomersGrowth = "+10%"
            };
        }
        catch
        {
            return new SystemStatsViewModel
            {
                UsersGrowth = "0%",
                OrdersGrowth = "0%",
                RevenueGrowth = "0%",
                CustomersGrowth = "0%"
            };
        }
    }
}
