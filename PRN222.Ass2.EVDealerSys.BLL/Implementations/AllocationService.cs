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

    // Legacy method - Deprecated: Sử dụng CreateStaffRequestAsync thay thế
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


            var allocation = new VehicleAllocation
            {
                VehicleId = dto.VehicleId,
                ToDealerId = dto.ToDealerId,
                Quantity = dto.Quantity,
                RequestedColor = dto.RequestedColor,
                DesiredDeliveryDate = dto.DesiredDeliveryDate,
                Reason = dto.ReasonText ?? string.Empty,
                RequestDate = DateTime.Now,
                Status = (int)AllocationStatus.PendingManagerReview, 
                RequestedByUserId = dto.RequestedByUserId,
                FromLocationType = 1, // EVM Factory
            };

            var created = await _allocationRepo.CreateAsync(allocation);

            var message = isSufficient 
                ? "Tạo yêu cầu phân bổ thành công. Đủ hàng trong kho."
                : $"Tạo yêu cầu thành công. Lưu ý: Chỉ có {availableStock} xe trong kho.";

            return (true, message, created);
        }
        catch (Exception ex)
        {

            var innerMsg = ex.InnerException?.Message ?? ex.Message;
            return (false, $"Lỗi: {innerMsg}", null);
        }
    }

    // Role 3: Tạo yêu cầu sơ bộ
    public async Task<(bool Success, string Message, VehicleAllocation? Allocation)> CreateStaffRequestAsync(AllocationRequestDto dto)
    {
        try
        {
            if (dto.VehicleId <= 0) return (false, "Vehicle ID không hợp lệ", null);
            if (dto.ToDealerId <= 0) return (false, "Dealer ID không hợp lệ", null);
            if (dto.RequestedByUserId <= 0) return (false, "User ID không hợp lệ", null);
            if (dto.Quantity <= 0) return (false, "Số lượng phải lớn hơn 0", null);
            if (dto.DesiredDeliveryDate < DateTime.Now.Date) 
                return (false, "Thời hạn giao hàng phải trong tương lai", null);


            var vehicle = await _vehicleRepo.GetByIdAsync(dto.VehicleId);
            if (vehicle == null) return (false, $"Không tìm thấy xe có ID = {dto.VehicleId}", null);

            var dealer = await _dealerRepo.GetByIdAsync(dto.ToDealerId);
            if (dealer == null) return (false, $"Không tìm thấy đại lý có ID = {dto.ToDealerId}", null);

            var user = await _userRepo.GetByIdAsync(dto.RequestedByUserId);
            if (user == null || user.Role != 3) 
                return (false, "User không hợp lệ hoặc không phải Role 3", null);


            var allocation = new VehicleAllocation
            {
                VehicleId = dto.VehicleId,
                ToDealerId = dto.ToDealerId,
                Quantity = dto.Quantity,
                RequestedColor = dto.RequestedColor,
                DesiredDeliveryDate = dto.DesiredDeliveryDate,
                Reason = dto.ReasonText ?? string.Empty,
                RequestDate = DateTime.Now,
                Status = (int)AllocationStatus.PendingManagerReview, // Chờ Role 2
                RequestedByUserId = dto.RequestedByUserId,
                FromLocationType = 1
            };

            var created = await _allocationRepo.CreateAsync(allocation);
            return (true, "Gửi yêu cầu đến Manager thành công", created);
        }
        catch (Exception ex)
        {
            return (false, $"Lỗi: {ex.InnerException?.Message ?? ex.Message}", null);
        }
    }

    // Role 3: Xem yêu cầu của mình
    public async Task<IEnumerable<AllocationRequestDto>> GetStaffRequestsAsync(int staffUserId)
    {
        var allocations = await _allocationRepo.GetByUserIdAsync(staffUserId);
        return allocations.Select(MapToDto);
    }

    // Role 2: Lấy danh sách yêu cầu chờ xét duyệt
    public async Task<IEnumerable<AllocationRequestDto>> GetPendingManagerReviewAsync(int dealerId)
    {
        var allocations = await _allocationRepo.GetByDealerAndStatusAsync(
            dealerId, 
            (int)AllocationStatus.PendingManagerReview);
        return allocations.Select(MapToDto);
    }

    // Role 2: Xác nhận và chuyển lên EVM
    public async Task<(bool Success, string Message)> ManagerApproveAndForwardAsync(
        int allocationId, int managerId, string? notes)
    {
        try
        {
            var allocation = await _allocationRepo.GetByIdWithDetailsAsync(allocationId);
            if (allocation == null) return (false, "Không tìm thấy yêu cầu");

            if (allocation.Status != (int)AllocationStatus.PendingManagerReview)
                return (false, "Yêu cầu không ở trạng thái chờ Manager duyệt");

            var manager = await _userRepo.GetByIdAsync(managerId);
            if (manager == null || manager.Role != 2)
                return (false, "Manager không hợp lệ");


            allocation.Status = (int)AllocationStatus.PendingEVMApproval;
            allocation.ReviewedByUserId = managerId;
            allocation.ReviewDate = DateTime.Now;
            allocation.ManagerNotes = notes;

            await _allocationRepo.UpdateAsync(allocation);

            return (true, "Đã xác nhận và chuyển yêu cầu lên EVM");
        }
        catch (Exception ex)
        {
            return (false, $"Lỗi: {ex.InnerException?.Message ?? ex.Message}");
        }
    }

    // Role 2: Từ chối yêu cầu từ Role 3
    public async Task<(bool Success, string Message)> ManagerRejectAsync(
        int allocationId, int managerId, string reason)
    {
        try
        {
            var allocation = await _allocationRepo.GetByIdWithDetailsAsync(allocationId);
            if (allocation == null) return (false, "Không tìm thấy yêu cầu");

            if (allocation.Status != (int)AllocationStatus.PendingManagerReview)
                return (false, "Yêu cầu không ở trạng thái chờ Manager duyệt");

            allocation.Status = (int)AllocationStatus.Rejected;
            allocation.ReviewedByUserId = managerId;
            allocation.ReviewDate = DateTime.Now;
            allocation.ManagerNotes = reason;

            await _allocationRepo.UpdateAsync(allocation);

            return (true, "Đã từ chối yêu cầu");
        }
        catch (Exception ex)
        {
            return (false, $"Lỗi: {ex.InnerException?.Message ?? ex.Message}");
        }
    }

    // Role 4: Lấy danh sách yêu cầu chờ EVM duyệt
    public async Task<IEnumerable<AllocationRequestDto>> GetPendingEVMApprovalAsync()
    {
        var allocations = await _allocationRepo.GetByStatusAsync(
            (int)AllocationStatus.PendingEVMApproval);
        return allocations.Select(MapToDto);
    }

    // Role 4: Phê duyệt yêu cầu
    public async Task<(bool Success, string Message)> EVMApproveRequestAsync(
        int allocationId, int approvedByUserId, string? notes, string? suggestion)
    {
        try
        {
            var allocation = await _allocationRepo.GetByIdWithDetailsAsync(allocationId);
            if (allocation == null) return (false, "Không tìm thấy yêu cầu");

            if (allocation.Status != (int)AllocationStatus.PendingEVMApproval)
                return (false, "Yêu cầu chưa được Manager xác nhận");

            // Check stock
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

    // Role 4: Từ chối yêu cầu
    public async Task<(bool Success, string Message)> EVMRejectRequestAsync(
        int allocationId, int rejectedByUserId, string reason)
    {
        try
        {
            var allocation = await _allocationRepo.GetByIdWithDetailsAsync(allocationId);
            if (allocation == null) return (false, "Không tìm thấy yêu cầu");

            if (allocation.Status != (int)AllocationStatus.PendingEVMApproval)
                return (false, "Yêu cầu không ở trạng thái chờ EVM duyệt");

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

            // FIX: Kiểm tra cả 2 trạng thái có thể
            if (allocation.Status != (int)AllocationStatus.PendingManagerReview && 
                allocation.Status != (int)AllocationStatus.PendingEVMApproval)
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

            // FIX: Kiểm tra cả 2 trạng thái có thể
            if (allocation.Status != (int)AllocationStatus.PendingManagerReview && 
                allocation.Status != (int)AllocationStatus.PendingEVMApproval)
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

            if (allocation.Status != (int)AllocationStatus.Approved)
                return (false, "Chỉ có thể vận chuyển yêu cầu đã được phê duyệt");

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

            if (allocation.Status != (int)AllocationStatus.InTransit)
                return (false, "Chỉ có thể giao hàng khi đang vận chuyển");

            // Chỉ đánh dấu "Đã giao" - chưa cập nhật kho
            allocation.Status = (int)AllocationStatus.Delivered;
            allocation.DeliveryDate = deliveryDate;
            await _allocationRepo.UpdateAsync(allocation);

            return (true, "Đã giao hàng. Chờ dealer xác nhận nhận hàng.");
        }
        catch (Exception ex)
        {
            return (false, $"Lỗi: {ex.InnerException?.Message ?? ex.Message}");
        }
    }

    public async Task<(bool Success, string Message)> ConfirmReceivedAsync(int allocationId, int userId)
    {
        try
        {
            var allocation = await _allocationRepo.GetByIdWithDetailsAsync(allocationId);
            if (allocation == null)
                return (false, "Không tìm thấy yêu cầu");

            if (allocation.Status != (int)AllocationStatus.Delivered)
                return (false, "Chỉ có thể xác nhận nhận hàng khi đã được giao");

            // Cập nhật kho dealer
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

            // Cập nhật status và thời gian nhận hàng thực tế
            allocation.Status = (int)AllocationStatus.Received;
            allocation.DeliveryDate = DateTime.Now; // Ghi đè thời gian thực tế nhận hàng
            await _allocationRepo.UpdateAsync(allocation);

            return (true, "Xác nhận nhận hàng thành công. Đã cập nhật kho đại lý.");
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
        0 => "Chờ Manager xét duyệt",
        1 => "Chờ EVM phê duyệt",
        2 => "Đã phê duyệt",
        3 => "Từ chối",
        4 => "Đang vận chuyển",
        5 => "Đã giao hàng",
        6 => "Đã nhận hàng",  // Changed from "Đã hủy"
        7 => "Đã hủy",
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
            ReviewedByUserId = allocation.ReviewedByUserId,
            ReviewedByUserName = allocation.ReviewedByUser?.Name,
            ReviewDate = allocation.ReviewDate,
            ManagerNotes = allocation.ManagerNotes,
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


    public async Task<IEnumerable<AllocationRequestDto>> GetManagerForwardedRequestsAsync(int dealerId)
    {

        
        var allocations = await _allocationRepo.GetByDealerIdAsync(dealerId);
        
        return allocations
            .Where(a => a.ReviewedByUserId != null && a.Status >= 1)
            .Select(MapToDto)
            .OrderByDescending(x => x.ReviewDate);
    }

    public async Task<IEnumerable<AllocationRequestDto>> GetAllEVMRequestsAsync()
    {
        var allocations = await _allocationRepo.GetAllWithDetailsAsync();
        return allocations.Select(MapToDto);
    }

    public async Task<bool> MarkAsReceivedAsync(int allocationId, int userId)
    {
        var (success, _) = await ConfirmReceivedAsync(allocationId, userId);
        return success;
    }
}
