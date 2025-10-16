using PRN222.Ass2.EVDealerSys.BusinessObjects.DTO.Allocation;
using PRN222.Ass2.EVDealerSys.BusinessObjects.Models;

namespace PRN222.Ass2.EVDealerSys.BLL.Interfaces;

public interface IAllocationService
{
    // Dealer Manager Functions
    Task<(bool Success, string Message, VehicleAllocation? Allocation)> CreateRequestAsync(AllocationRequestDto dto);
    Task<IEnumerable<AllocationRequestDto>> GetDealerRequestsAsync(int dealerId);
    
    // EVM Staff/Admin Functions
    Task<IEnumerable<AllocationRequestDto>> GetPendingRequestsAsync();
    Task<(bool Success, string Message)> ApproveRequestAsync(int allocationId, int approvedByUserId, string? notes, string? suggestion);
    Task<(bool Success, string Message)> RejectRequestAsync(int allocationId, int rejectedByUserId, string reason);
    
    // Allocation Transaction
    Task<(bool Success, string Message)> FulfillAllocationAsync(int allocationId);
    Task<(bool Success, string Message)> UpdateShipmentStatusAsync(int allocationId, DateTime shipmentDate);
    Task<(bool Success, string Message)> UpdateDeliveryStatusAsync(int allocationId, DateTime deliveryDate);
    
    // Stock Check
    Task<(int AvailableStock, bool IsSufficient)> CheckStockAvailabilityAsync(int vehicleId, int quantity, string? color);
    
    // Helper
    string GetStatusText(int status);
    string GetReasonText(int reason);
}
