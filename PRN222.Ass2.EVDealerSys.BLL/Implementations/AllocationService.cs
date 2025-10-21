using PRN222.Ass2.EVDealerSys.BLL.Interfaces;
using PRN222.Ass2.EVDealerSys.BusinessObjects.DTO.Allocation;
using PRN222.Ass2.EVDealerSys.BusinessObjects.Enums;
using PRN222.Ass2.EVDealerSys.BusinessObjects.Models;
using PRN222.Ass2.EVDealerSys.DAL.Interfaces;

namespace PRN222.Ass2.EVDealerSys.BLL.Implementations;

public class AllocationService : IAllocationService
{
    private readonly IVehicleAllocationRepository _allocationRepo;
    private readonly IInventoryRepository _inventoryRepo;
    private readonly IVehicleRepository _vehicleRepo;

    public AllocationService(
        IVehicleAllocationRepository allocationRepo,
        IInventoryRepository inventoryRepo,
        IVehicleRepository vehicleRepo)
    {
        _allocationRepo = allocationRepo;
        _inventoryRepo = inventoryRepo;
        _vehicleRepo = vehicleRepo;
    }

    public async Task<(bool Success, string Message, VehicleAllocation? Allocation)> CreateRequestAsync(AllocationRequestDto dto)
    {
        try
        {
            // Validate
            if (dto.Quantity <= 0)
                return (false, "Số lượng phải lớn hơn 0", null);

            if (dto.DesiredDeliveryDate < DateTime.Now)
                return (false, "Thời hạn giao hàng phải trong tương lai", null);

            // Check vehicle exists
            var vehicle = await _vehicleRepo.GetByIdAsync(dto.VehicleId);
            if (vehicle == null)
                return (false, "Xe không tồn tại", null);

            // Check stock availability
            var (availableStock, isSufficient) = await CheckStockAvailabilityAsync(dto.VehicleId, dto.Quantity, dto.RequestedColor);

            var allocation = new VehicleAllocation
            {
                VehicleId = dto.VehicleId,
                ToDealerId = dto.ToDealerId,
                Quantity = dto.Quantity,
                RequestedColor = dto.RequestedColor,
                DesiredDeliveryDate = dto.DesiredDeliveryDate,
                Reason = dto.ReasonText,
                RequestDate = DateTime.Now,
                Status = (int)AllocationStatus.Pending,
                RequestedByUserId = dto.RequestedByUserId,
                FromLocationType = 1 // EVM Factory
            };

            var created = await _allocationRepo.CreateAsync(allocation);

            var message = isSufficient 
                ? "Tạo yêu cầu phân bổ thành công. Đủ hàng trong kho."
                : $"Tạo yêu cầu thành công. Lưu ý: Chỉ có {availableStock} xe trong kho.";

            return (true, message, created);
        }
        catch (Exception ex)
        {
            return (false, $"Lỗi: {ex.Message}", null);
        }
    }

    public async Task<IEnumerable<AllocationRequestDto>> GetDealerRequestsAsync(int dealerId)
    {
        var allocations = await _allocationRepo.GetByDealerIdAsync(dealerId);
        return allocations.Select(MapToDto);
    }

    public async Task<IEnumerable<AllocationRequestDto>> GetPendingRequestsAsync()
    {
        var allocations = await _allocationRepo.GetPendingRequestsAsync();
        return allocations.Select(MapToDto);
    }

    public async Task<(bool Success, string Message)> ApproveRequestAsync(int allocationId, int approvedByUserId, string? notes, string? suggestion)
    {
        try
        {
            var allocation = await _allocationRepo.GetByIdWithDetailsAsync(allocationId);
            if (allocation == null)
                return (false, "Không tìm thấy yêu cầu");

            if (allocation.Status != (int)AllocationStatus.Pending)
                return (false, "Yêu cầu đã được xử lý");

            // Check stock again
            var hasSufficient = await _allocationRepo.HasSufficientStockAsync(
                allocation.VehicleId ?? 0, 
                allocation.Quantity ?? 0, 
                allocation.RequestedColor);

            if (!hasSufficient)
                return (false, "Không đủ hàng trong kho để phê duyệt");

            allocation.Status = (int)AllocationStatus.Approved;
            allocation.ApprovedByUserId = approvedByUserId;
            allocation.ApprovalNotes = notes;
            allocation.StaffSuggestion = suggestion;
            allocation.AllocationDate = DateTime.Now;

            await _allocationRepo.UpdateAsync(allocation);

            return (true, "Phê duyệt yêu cầu thành công");
        }
        catch (Exception ex)
        {
            return (false, $"Lỗi: {ex.Message}");
        }
    }

    public async Task<(bool Success, string Message)> RejectRequestAsync(int allocationId, int rejectedByUserId, string reason)
    {
        try
        {
            var allocation = await _allocationRepo.GetByIdWithDetailsAsync(allocationId);
            if (allocation == null)
                return (false, "Không tìm thấy yêu cầu");

            if (allocation.Status != (int)AllocationStatus.Pending)
                return (false, "Yêu cầu đã được xử lý");

            allocation.Status = (int)AllocationStatus.Rejected;
            allocation.ApprovedByUserId = rejectedByUserId;
            allocation.ApprovalNotes = reason;

            await _allocationRepo.UpdateAsync(allocation);

            return (true, "Từ chối yêu cầu thành công");
        }
        catch (Exception ex)
        {
            return (false, $"Lỗi: {ex.Message}");
        }
    }

    public async Task<(bool Success, string Message)> FulfillAllocationAsync(int allocationId)
    {
        try
        {
            var allocation = await _allocationRepo.GetByIdWithDetailsAsync(allocationId);
            if (allocation == null)
                return (false, "Không tìm thấy yêu cầu");

            if (allocation.Status != (int)AllocationStatus.Approved)
                return (false, "Yêu cầu chưa được phê duyệt");

            // Reduce EVM stock - FIX: Không dùng await với GetEvmStock vì nó sync
            var evmStock = _inventoryRepo.GetEvmStock(allocation.VehicleId ?? 0);
            if (evmStock == null || evmStock.Quantity < allocation.Quantity)
                return (false, "Không đủ hàng trong kho");

            evmStock.Quantity -= allocation.Quantity ?? 0;
            _inventoryRepo.Update(evmStock);
            await _inventoryRepo.SaveAsync(); // Save changes

            // Update status
            allocation.Status = (int)AllocationStatus.InTransit;
            allocation.ShipmentDate = DateTime.Now;

            await _allocationRepo.UpdateAsync(allocation);

            return (true, "Xuất kho thành công. Xe đang vận chuyển.");
        }
        catch (Exception ex)
        {
            return (false, $"Lỗi: {ex.Message}");
        }
    }

    public async Task<(bool Success, string Message)> UpdateShipmentStatusAsync(int allocationId, DateTime shipmentDate)
    {
        var allocation = await _allocationRepo.GetByIdAsync(allocationId);
        if (allocation == null)
            return (false, "Không tìm thấy yêu cầu");

        allocation.Status = (int)AllocationStatus.InTransit;
        allocation.ShipmentDate = shipmentDate;
        await _allocationRepo.UpdateAsync(allocation);

        return (true, "Cập nhật trạng thái vận chuyển thành công");
    }

    public async Task<(bool Success, string Message)> UpdateDeliveryStatusAsync(int allocationId, DateTime deliveryDate)
    {
        try
        {
            var allocation = await _allocationRepo.GetByIdWithDetailsAsync(allocationId);
            if (allocation == null)
                return (false, "Không tìm thấy yêu cầu");

            // Add to dealer inventory - FIX: Sử dụng giá trị số thay vì enum
            var dealerStock = _inventoryRepo.GetByVehicle(
                allocation.VehicleId ?? 0, 
                3, // DealerStock = 3
                allocation.ToDealerId);

            if (dealerStock == null)
            {
                dealerStock = new Inventory
                {
                    VehicleId = allocation.VehicleId,
                    DealerId = allocation.ToDealerId,
                    LocationType = 3, // DealerStock
                    Quantity = allocation.Quantity ?? 0
                };
                _inventoryRepo.PrepareCreate(dealerStock);
            }
            else
            {
                dealerStock.Quantity += allocation.Quantity ?? 0;
                _inventoryRepo.PrepareUpdate(dealerStock);
            }

            await _inventoryRepo.SaveAsync();

            allocation.Status = (int)AllocationStatus.Delivered;
            allocation.DeliveryDate = deliveryDate;
            await _allocationRepo.UpdateAsync(allocation);

            return (true, "Giao hàng thành công. Đã cập nhật kho đại lý.");
        }
        catch (Exception ex)
        {
            return (false, $"Lỗi: {ex.Message}");
        }
    }

    public async Task<(int AvailableStock, bool IsSufficient)> CheckStockAvailabilityAsync(int vehicleId, int quantity, string? color)
    {
        var availableStock = await _allocationRepo.GetAvailableStockAsync(vehicleId, color);
        return (availableStock, availableStock >= quantity);
    }

    public string GetStatusText(int status) => status switch
    {
        0 => "Chờ duyệt",
        1 => "Đã duyệt",
        2 => "Từ chối",
        3 => "Đang vận chuyển",
        4 => "Đã giao",
        5 => "Đã hủy",
        _ => "Không xác định"
    };

    public string GetReasonText(int reason) => reason switch
    {
        1 => "Đáp ứng đơn hàng",
        2 => "Dự trữ kho",
        3 => "Test drive",
        4 => "Trưng bày",
        _ => "Khác"
    };

    private AllocationRequestDto MapToDto(VehicleAllocation allocation)
    {
        return new AllocationRequestDto
        {
            Id = allocation.Id,
            VehicleId = allocation.VehicleId ?? 0,
            VehicleModel = allocation.Vehicle?.Model,
            VehicleVersion = allocation.Vehicle?.Version,
            Quantity = allocation.Quantity ?? 0,
            RequestedColor = allocation.RequestedColor,
            DesiredDeliveryDate = allocation.DesiredDeliveryDate ?? DateTime.Now,
            Reason = int.TryParse(allocation.Reason, out int r) ? r : 0,
            ReasonText = allocation.Reason,
            ToDealerId = allocation.ToDealerId ?? 0,
            DealerName = allocation.ToDealer?.Name,
            RequestedByUserId = allocation.RequestedByUserId ?? 0,
            RequestedByUserName = allocation.RequestedByUser?.Name,
            RequestDate = allocation.RequestDate ?? DateTime.Now,
            Status = allocation.Status ?? 0,
            StatusText = GetStatusText(allocation.Status ?? 0),
            ApprovedByUserId = allocation.ApprovedByUserId,
            ApprovalNotes = allocation.ApprovalNotes,
            StaffSuggestion = allocation.StaffSuggestion,
            AllocationDate = allocation.AllocationDate,
            ShipmentDate = allocation.ShipmentDate,
            DeliveryDate = allocation.DeliveryDate
        };
    }
}
