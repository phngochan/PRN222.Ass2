namespace PRN222.Ass2.EVDealerSys.Models.Dashboard;

// Staff Dashboard ViewModel
public class StaffDashboardViewModel : BaseDashboardViewModel
{
    public int MyOrdersToday { get; set; }
    public int MyOrdersThisWeek { get; set; }
    public int MyOrdersThisMonth { get; set; }
    public List<TaskViewModel> PendingTasks { get; set; } = new List<TaskViewModel>();
    public List<ActivityViewModel> RecentActivities { get; set; } = new List<ActivityViewModel>();
}

