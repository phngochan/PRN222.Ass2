namespace PRN222.Ass2.EVDealerSys.BusinessObjects.Enums;

public enum AllocationStatus
{
    Pending = 0,           // Đang chờ duyệt
    Approved = 1,          // Đã phê duyệt
    Rejected = 2,          // Bị từ chối
    InTransit = 3,         // Đang vận chuyển
    Delivered = 4,         // Đã giao hàng
    Cancelled = 5          // Đã hủy
}

public enum AllocationReason
{
    ForOrder = 1,          // Đáp ứng đơn hàng
    ForStock = 2,          // Dự trữ kho
    ForTestDrive = 3,      // Test drive
    ForDisplay = 4,         // Trưng bày
    Pending = 0,
    Approved = 1,
    Shipped = 2,
    Received = 3
}
