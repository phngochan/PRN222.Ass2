namespace PRN222.Ass2.EVDealerSys.Models.Dashboard;

public class BaseDashboardViewModel
{
    public string UserName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public DateTime LoginTime { get; set; } = DateTime.Now;
}
