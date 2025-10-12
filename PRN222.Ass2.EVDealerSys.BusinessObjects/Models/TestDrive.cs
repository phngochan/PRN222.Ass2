namespace PRN222.Ass2.EVDealerSys.BusinessObjects.Models;

public partial class TestDrive
{
    public int Id { get; set; }

    public int? CustomerId { get; set; }

    public int? VehicleId { get; set; }

    public int? DealerId { get; set; }

    public int? UserId { get; set; }

    public DateTime? ScheduledDate { get; set; }

    public DateTime? StartTime { get; set; }

    public DateTime? EndTime { get; set; }

    public int? Status { get; set; } // 1: Pending, 2: Confirmed, 3: Completed, 4: Cancelled

    public string? Notes { get; set; }

    public string? CustomerName { get; set; }

    public string? CustomerPhone { get; set; }

    public string? CustomerEmail { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Customer? Customer { get; set; }

    public virtual Dealer? Dealer { get; set; }

    public virtual User? User { get; set; }

    public virtual Vehicle? Vehicle { get; set; }
}
