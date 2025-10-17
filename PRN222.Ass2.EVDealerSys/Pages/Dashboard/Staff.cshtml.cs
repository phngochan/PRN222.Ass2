using PRN222.Ass2.EVDealerSys.Base.BasePageModels;
using PRN222.Ass2.EVDealerSys.BLL.Interfaces;
using PRN222.Ass2.EVDealerSys.Models.Dashboard;

namespace PRN222.Ass2.EVDealerSys.Pages.Dashboard
{
    public class StaffModel : BaseViewOnlyPageModel<StaffDashboardViewModel>
    {
        private readonly IOrderService _orderService;

        public StaffModel(IOrderService orderService)
        {
            _orderService = orderService;
        }

        public override async Task OnGetAsync()
        {
            ViewModel = new StaffDashboardViewModel
            {
                UserName = User.Identity?.Name ?? "Staff",
                MyOrdersToday = await _orderService.GetOrdersCountByUserAndDateRangeAsync(
                    GetUserId(), DateTime.Today, DateTime.Today.AddDays(1)
                ),
            };
        }

        private int GetUserId()
        {
            return int.TryParse(User?.FindFirst("Id")?.Value, out var id) ? id : 0;
        }
    }
}
