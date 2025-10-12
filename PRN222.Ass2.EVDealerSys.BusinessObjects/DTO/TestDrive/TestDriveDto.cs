namespace PRN222.Ass2.EVDealerSys.BusinessObjects.DTO.TestDrive;
public class TestDriveDto
{
    public int Id { get; set; }
    public int VehicleId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public DateTime ScheduledDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public string? Notes { get; set; }
    public int? CustomerId { get; set; }
    public int? DealerId { get; set; }
    public int? UserId { get; set; }
    public int Status { get; set; }

    // Display properties
    public string? VehicleName { get; set; }
    public string? VehicleModel { get; set; }
    public string? DealerName { get; set; }
    public string? UserName { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public DateTime? CreatedAt { get; set; }
}
