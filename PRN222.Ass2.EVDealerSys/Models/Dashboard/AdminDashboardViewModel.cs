namespace PRN222.Ass2.EVDealerSys.Models.Dashboard;

public class AdminDashboardViewModel : BaseDashboardViewModel
{
    public int TotalUsers { get; set; }
    public string UsersGrowth { get; set; } = "0%";
    
    public int TotalDealers { get; set; }
    public string DealersGrowth { get; set; } = "0%";
    
    public int TotalOrders { get; set; }
    public string OrdersGrowth { get; set; } = "0%";
    
    public int TotalVehicles { get; set; }
    public string VehiclesGrowth { get; set; } = "0%";
    
    public List<RecentUserViewModel> RecentUsers { get; set; } = new List<RecentUserViewModel>();
    public SystemStatsViewModel SystemStats { get; set; } = new SystemStatsViewModel();
}

