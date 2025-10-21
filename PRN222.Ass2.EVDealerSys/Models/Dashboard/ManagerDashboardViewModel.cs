namespace PRN222.Ass2.EVDealerSys.Models.Dashboard;

// Manager Dashboard ViewModel
public class ManagerDashboardViewModel : BaseDashboardViewModel
{
    public int? MyDealerId { get; set; }
    public int StaffCount { get; set; }
    public int OrdersThisMonth { get; set; }
    public int CustomersThisMonth { get; set; }
    public List<RecentOrderViewModel> RecentOrders { get; set; } = new List<RecentOrderViewModel>();
    public List<TeamMemberViewModel> TeamMembers { get; set; } = new List<TeamMemberViewModel>();
}

