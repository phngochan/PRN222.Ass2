namespace PRN222.Ass2.EVDealerSys.BusinessObjects.Models;

public partial class VehicleAllocation
{
    public int Id { get; set; }

    public int? VehicleId { get; set; }

    public int? FromLocationType { get; set; } // 1: EVM Factory, 2: Regional Warehouse

    public int? ToDealerId { get; set; }

    public int? Quantity { get; set; }

    public string? RequestedColor { get; set; } // Màu mong muốn

    public DateTime? DesiredDeliveryDate { get; set; } // Thời hạn mong muốn

    public string? Reason { get; set; } // Lý do: Order, Stock, TestDrive...

    public DateTime? RequestDate { get; set; }

    public DateTime? AllocationDate { get; set; } // Ngày phê duyệt

    public DateTime? ShipmentDate { get; set; } // Ngày xuất kho

    public DateTime? DeliveryDate { get; set; } // Ngày giao hàng

    public int? Status { get; set; } // 0: Pending, 1: Approved, 2: Rejected, 3: In Transit, 4: Delivered

    public int? RequestedByUserId { get; set; } // User tạo request (Role 3 - Dealer Staff)

    public int? ReviewedByUserId { get; set; } // Role 2 - Dealer Manager xác nhận

    public DateTime? ReviewDate { get; set; } // Ngày Role 2 xác nhận

    public string? ManagerNotes { get; set; } // Ghi chú của Manager

    public int? ApprovedByUserId { get; set; } // EVM Staff/Admin phê duyệt (Role 4)

    public string? ApprovalNotes { get; set; } // Ghi chú khi phê duyệt/từ chối

    public string? StaffSuggestion { get; set; } // Đề xuất của EVM Staff

    public virtual Dealer? ToDealer { get; set; }

    public virtual Vehicle? Vehicle { get; set; }

    public virtual User? RequestedByUser { get; set; }

    public virtual User? ReviewedByUser { get; set; }

    public virtual User? ApprovedByUser { get; set; }
}
