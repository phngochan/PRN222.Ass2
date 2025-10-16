using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using PRN222.Ass2.EVDealerSys.BLL.Interfaces;

namespace PRN222.Ass2.EVDealerSys.Pages.Dashboard
{
    public class AdminModel : PageModel
    {
        private readonly IUserService _userService;
        private readonly IDealerService _dealerService;
        private readonly IOrderService _orderService;
        private readonly IVehicleService _vehicleService;
        private readonly ICustomerService _customerService;

        public AdminDashboardViewModel ViewModel { get; set; } = new();

        public AdminModel(
            IUserService userService,
            IDealerService dealerService,
            IOrderService orderService,
            IVehicleService vehicleService,
            ICustomerService customerService)
        {
            _userService = userService;
            _dealerService = dealerService;
            _orderService = orderService;
            _vehicleService = vehicleService;
            _customerService = customerService;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            // Nếu bạn có phương thức kiểm tra role, dùng nó ở đây (ví dụ lấy từ claim)
            var role = HttpContext.User.FindFirst("Role")?.Value;
            if (role != "1")
            {
                return Forbid();
            }

            // Gán dữ liệu
            ViewModel.UserName = User.Identity?.Name ?? "Admin";
            ViewModel.TotalUsers = await GetTotalUsersCount();
            ViewModel.TotalDealers = await GetTotalDealersCount();
            ViewModel.TotalOrders = await GetTotalOrdersCount();
            ViewModel.TotalVehicles = await GetTotalVehiclesCount();
            ViewModel.RecentUsers = await GetRecentUsers();
            ViewModel.SystemStats = await GetSystemStats();

            return Page();
        }

        // ========================== PRIVATE HELPERS ==========================

        private async Task<int> GetTotalUsersCount()
        {
            try
            {
                var users = await _userService.GetAllUsersAsync();
                return users.Count;
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
}
