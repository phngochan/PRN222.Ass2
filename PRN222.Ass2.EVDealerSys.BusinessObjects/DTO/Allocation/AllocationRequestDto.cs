namespace PRN222.Ass2.EVDealerSys.BusinessObjects.DTO.Allocation;

public class AllocationRequestDto
{
    public int Id { get; set; }
    public int VehicleId { get; set; }
    public string? VehicleModel { get; set; }
    public string? VehicleVersion { get; set; }
    public int Quantity { get; set; }
    public string? RequestedColor { get; set; }
    public DateTime DesiredDeliveryDate { get; set; }
    public int Reason { get; set; }
    public string? ReasonText { get; set; }
    public int ToDealerId { get; set; }
    public string? DealerName { get; set; }
    
    // Role 3 info
    public int RequestedByUserId { get; set; }
    public string? RequestedByUserName { get; set; }
    public DateTime RequestDate { get; set; }
    
    // Role 2 review
    public int? ReviewedByUserId { get; set; }
    public string? ReviewedByUserName { get; set; }
    public DateTime? ReviewDate { get; set; }
    public string? ManagerNotes { get; set; }
    
    // Status
    public int Status { get; set; }
    public string? StatusText { get; set; }
    
    // Role 4 approval
    public int? ApprovedByUserId { get; set; }
    public string? ApprovedByUserName { get; set; }
    public string? ApprovalNotes { get; set; }
    public string? StaffSuggestion { get; set; }
    public DateTime? AllocationDate { get; set; }
    
    // Tracking
    public DateTime? ShipmentDate { get; set; }
    public DateTime? DeliveryDate { get; set; }
    
    // Inventory check
    public int? AvailableStock { get; set; }
    public bool IsStockSufficient { get; set; }
}
