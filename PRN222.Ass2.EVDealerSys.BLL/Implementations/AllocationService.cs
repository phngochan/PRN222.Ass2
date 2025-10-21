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
    private readonly IDealerRepository _dealerRepo;
    private readonly IUserRepository _userRepo;

    public AllocationService(
        IVehicleAllocationRepository allocationRepo,
        IInventoryRepository inventoryRepo,
        IVehicleRepository vehicleRepo,
        IDealerRepository dealerRepo,
        IUserRepository userRepo)
    {
        _allocationRepo = allocationRepo;
        _inventoryRepo = inventoryRepo;
        _vehicleRepo = vehicleRepo;
        _dealerRepo = dealerRepo;
        _userRepo = userRepo;
    }

    public async Task<(bool Success, string Message, VehicleAllocation? Allocation)> CreateRequestAsync(AllocationRequestDto dto)
    {
        try
        {
            // Validate basic fields
            if (dto.VehicleId <= 0)
                return (false, "Vehicle ID không hợp lệ", null);

            if (dto.ToDealerId <= 0)
                return (false, "Dealer ID không hợp lệ", null);

            if (dto.RequestedByUserId <= 0)
                return (false, "User ID không hợp lệ", null);

            if (dto.Quantity <= 0)
                return (false, "Số lượng phải lớn hơn 0", null);

            if (dto.DesiredDeliveryDate < DateTime.Now.Date)
                return (false, "Thời hạn giao hàng phải trong tương lai", null);

            // Verify vehicle exists
            var vehicle = await _vehicleRepo.GetByIdAsync(dto.VehicleId);
            if (vehicle == null)
                return (false, $"Không tìm thấy xe có ID = {dto.VehicleId}", null);

            // Verify dealer exists
            var dealer = await _dealerRepo.GetByIdAsync(dto.ToDealerId);
            if (dealer == null)
                return (false, $"Không tìm thấy đại lý có ID = {dto.ToDealerId}", null);

            // Verify user exists
            var user = await _userRepo.GetByIdAsync(dto.RequestedByUserId);
            if (user == null)
                return (false, $"Không tìm thấy user có ID = {dto.RequestedByUserId}", null);

            // Check stock availability
            var (availableStock, isSufficient) = await CheckStockAvailabilityAsync(dto.VehicleId, dto.Quantity, dto.RequestedColor);

            // Create allocation entity - KHÔNG set navigation properties
            var allocation = new VehicleAllocation
            {
                VehicleId = dto.VehicleId,
                ToDealerId = dto.ToDealerId,
                Quantity = dto.Quantity,
                RequestedColor = dto.RequestedColor,
                DesiredDeliveryDate = dto.DesiredDeliveryDate,
                Reason = dto.ReasonText ?? string.Empty,
                RequestDate = DateTime.Now,
                Status = (int)AllocationStatus.Pending,
                RequestedByUserId = dto.RequestedByUserId,
                FromLocationType = 1, // EVM Factory
                // KHÔNG set Vehicle, ToDealer, RequestedByUser để tránh EF tracking issues
            };

            var created = await _allocationRepo.CreateAsync(allocation);

            var message = isSufficient 
                ? "Tạo yêu cầu phân bổ thành công. Đủ hàng trong kho."
                : $"Tạo yêu cầu thành công. Lưu ý: Chỉ có {availableStock} xe trong kho.";

            return (true, message, created);
        }
        catch (Exception ex)
        {
            // Log chi tiết inner exception
            var innerMsg = ex.InnerException?.Message ?? ex.Message;
            return (false, $"Lỗi: {innerMsg}", null);
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
            return (false, $"Lỗi: {ex.InnerException?.Message ?? ex.Message}");
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
            return (false, $"Lỗi: {ex.InnerException?.Message ?? ex.Message}");
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

            // Reduce EVM stock
            var evmStock = _inventoryRepo.GetEvmStock(allocation.VehicleId ?? 0);
            if (evmStock == null || evmStock.Quantity < allocation.Quantity)
                return (false, "Không đủ hàng trong kho");

            evmStock.Quantity -= allocation.Quantity ?? 0;
            _inventoryRepo.Update(evmStock);
            await _inventoryRepo.SaveAsync();

            // Update status
            allocation.Status = (int)AllocationStatus.InTransit;
            allocation.ShipmentDate = DateTime.Now;

            await _allocationRepo.UpdateAsync(allocation);

            return (true, "Xuất kho thành công. Xe đang vận chuyển.");
        }
        catch (Exception ex)
        {
            return (false, $"Lỗi: {ex.InnerException?.Message ?? ex.Message}");
        }
    }

    public async Task<(bool Success, string Message)> UpdateShipmentStatusAsync(int allocationId, DateTime shipmentDate)
    {
        try
        {
            var allocation = await _allocationRepo.GetByIdAsync(allocationId);
            if (allocation == null)
                return (false, "Không tìm thấy yêu cầu");

            allocation.Status = (int)AllocationStatus.InTransit;
            allocation.ShipmentDate = shipmentDate;
            await _allocationRepo.UpdateAsync(allocation);

            return (true, "Cập nhật trạng thái vận chuyển thành công");
        }
        catch (Exception ex)
        {
            return (false, $"Lỗi: {ex.InnerException?.Message ?? ex.Message}");
        }
    }

    public async Task<(bool Success, string Message)> UpdateDeliveryStatusAsync(int allocationId, DateTime deliveryDate)
    {
        try
        {
            var allocation = await _allocationRepo.GetByIdWithDetailsAsync(allocationId);
            if (allocation == null)
                return (false, "Không tìm thấy yêu cầu");

            // Add to dealer inventory
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
                    LocationType = 3,
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
            return (false, $"Lỗi: {ex.InnerException?.Message ?? ex.Message}");
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
