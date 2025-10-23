using PRN222.Ass2.EVDealerSys.BusinessObjects.DTO.Allocation;
using PRN222.Ass2.EVDealerSys.BusinessObjects.Models;

namespace PRN222.Ass2.EVDealerSys.BLL.Interfaces;

public interface IAllocationService
{

    public Task<(bool Success, string Message, VehicleAllocation? Allocation)> CreateStaffRequestAsync(AllocationRequestDto dto);

    public Task<IEnumerable<AllocationRequestDto>> GetStaffRequestsAsync(int staffUserId);
    

    public Task<IEnumerable<AllocationRequestDto>> GetPendingManagerReviewAsync(int dealerId);
    

    public Task<(bool Success, string Message)> ManagerApproveAndForwardAsync(int allocationId, int managerId, string? notes);
    

    public Task<(bool Success, string Message)> ManagerRejectAsync(int allocationId, int managerId, string reason);
    

    public Task<IEnumerable<AllocationRequestDto>> GetManagerForwardedRequestsAsync(int dealerId);


    public Task<IEnumerable<AllocationRequestDto>> GetPendingEVMApprovalAsync();
    

    public Task<(bool Success, string Message)> EVMApproveRequestAsync(int allocationId, int approvedByUserId, string? notes, string? suggestion);
    

    public Task<(bool Success, string Message)> EVMRejectRequestAsync(int allocationId, int rejectedByUserId, string reason);

    public Task<IEnumerable<AllocationRequestDto>> GetAllEVMRequestsAsync();

    public Task<IEnumerable<AllocationRequestDto>> GetDealerRequestsAsync(int dealerId);
    public Task<(bool Success, string Message)> FulfillAllocationAsync(int allocationId);
    public Task<(bool Success, string Message)> UpdateShipmentStatusAsync(int allocationId, DateTime shipmentDate);
    public Task<(bool Success, string Message)> UpdateDeliveryStatusAsync(int allocationId, DateTime deliveryDate);
    public Task<(int AvailableStock, bool IsSufficient)> CheckStockAvailabilityAsync(int vehicleId, int quantity, string? color);
    public string GetStatusText(int status);
    public string GetReasonText(int reason);

}
