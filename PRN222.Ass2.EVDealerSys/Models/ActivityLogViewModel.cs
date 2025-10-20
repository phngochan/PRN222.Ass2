namespace PRN222.Ass2.EVDealerSys.Models;
public class ActivityLogViewModel
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
}
