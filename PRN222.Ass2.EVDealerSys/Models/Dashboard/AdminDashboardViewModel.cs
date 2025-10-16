namespace PRN222.Ass2.EVDealerSys.Models.Dashboard;

public class AdminDashboardViewModel : BaseDashboardViewModel
{
    public int TotalUsers { get; set; }
    public int TotalDealers { get; set; }
    public int TotalOrders { get; set; }
    public int TotalVehicles { get; set; }
    public List<RecentUserViewModel> RecentUsers { get; set; } = new List<RecentUserViewModel>();
    public SystemStatsViewModel SystemStats { get; set; } = new SystemStatsViewModel();
}

