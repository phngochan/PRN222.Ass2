        namespace PRN222.Ass2.EVDealerSys.BusinessObjects.Enums;

        public enum AllocationStatus
        {
            PendingManagerReview = 0,  // Chờ Dealer Manager xem xét (Role 3 → Role 2)
            PendingEVMApproval = 1,    // Chờ EVM duyệt (Role 2 → Role 4)
            Approved = 2,              // EVM đã phê duyệt
            Rejected = 3,              // Bị từ chối (có thể bởi Role 2 hoặc Role 4)
            InTransit = 4,             // Đang vận chuyển
            Delivered = 5,             // Đã giao hàng
            Cancelled = 6              // Đã hủy
        }

        public enum AllocationReason
        {
            ForOrder = 1,          // Đáp ứng đơn hàng
            ForStock = 2,          // Dự trữ kho
            ForTestDrive = 3,      // Test drive
            ForDisplay = 4         // Trưng bày
        }
