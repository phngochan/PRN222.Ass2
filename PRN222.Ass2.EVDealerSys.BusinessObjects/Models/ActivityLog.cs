namespace PRN222.Ass2.EVDealerSys.BusinessObjects.Models;
public class ActivityLog
{
    public int Id { get; set; }

    public int? UserId { get; set; }

    public string Action { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public virtual User? User { get; set; }
}
