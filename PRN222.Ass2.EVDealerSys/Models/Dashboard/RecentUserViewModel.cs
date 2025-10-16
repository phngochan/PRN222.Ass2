namespace PRN222.Ass2.EVDealerSys.Models.Dashboard;

// Supporting ViewModels
public class RecentUserViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int Role { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
}

