using PRN222.Ass2.EVDealerSys.BLL.Interfaces;
using PRN222.Ass2.EVDealerSys.BusinessObjects.DTO.Customer;
using PRN222.Ass2.EVDealerSys.BusinessObjects.DTO.TestDrive;
using PRN222.Ass2.EVDealerSys.BusinessObjects.Models;
using PRN222.Ass2.EVDealerSys.DAL.Interfaces;


namespace PRN222.Ass2.EVDealerSys.BLL.Implementations;
public class TestDriveService(ITestDriveRepository testDriveRepo, ICustomerService customerService) : ITestDriveService
{
    private readonly ITestDriveRepository _testDriveRepo = testDriveRepo;
    private readonly ICustomerService _customerService = customerService;

    public async Task<List<TestDriveItemDto>> GetAllAsync(int? dealerId = null)
    {
        var testDrives = await _testDriveRepo.GetAllAsync();

        if (dealerId.HasValue)
        {
            testDrives = testDrives.Where(t => t.DealerId == dealerId).ToList();
        }

        return testDrives.Select(MapToItemDto).ToList();
    }

    public async Task<TestDriveDto?> GetByIdAsync(int id)
    {
        var testDrive = await _testDriveRepo.GetByIdAsync(id);
        return testDrive == null ? null : MapToDto(testDrive);
    }

    public async Task<TestDriveDashboardDto> GetDashboardDataAsync(int? dealerId = null)
    {
        var allTestDrives = await _testDriveRepo.GetAllAsync();

        if (dealerId.HasValue)
        {
            allTestDrives = allTestDrives.Where(t => t.DealerId == dealerId).ToList();
        }

        var today = DateTime.Today;
        var todayTestDrives = allTestDrives.Where(t => t.ScheduledDate?.Date == today).ToList();
        var upcomingTestDrives = allTestDrives.Where(t => t.ScheduledDate?.Date > today && t.Status == 2).Take(5).ToList();

        return new TestDriveDashboardDto
        {
            TodayTestDrivesCount = todayTestDrives.Count,
            PendingTestDrives = allTestDrives.Count(t => t.Status == 1),
            CompletedTestDrives = allTestDrives.Count(t => t.Status == 3),
            TotalTestDrives = allTestDrives.Count(),
            UpcomingTestDrives = upcomingTestDrives.Select(MapToItemDto).ToList(),
            TodayTestDrives = todayTestDrives.Select(MapToItemDto).ToList()
        };
    }

    public async Task<List<TestDriveItemDto>> GetByDateAsync(DateTime date, int? dealerId = null)
    {
        var testDrives = await _testDriveRepo.GetByDateAsync(date);

        if (dealerId.HasValue)
        {
            testDrives = testDrives.Where(t => t.DealerId == dealerId).ToList();
        }

        return testDrives.Select(MapToItemDto).ToList();
    }

    public async Task<List<TestDriveItemDto>> SearchAsync(string? searchTerm, int? status, DateTime? date, int? vehicleId, int? dealerId = null)
    {
        var testDrives = await _testDriveRepo.SearchAsync(searchTerm, status, date, vehicleId, dealerId);
        return testDrives.Select(MapToItemDto).ToList();
    }

    public async Task<TestDrive> CreateAsync(TestDriveDto dto)
    {
        try
        {
            // If no CustomerId is provided, create a new customer
            if (!dto.CustomerId.HasValue && !string.IsNullOrEmpty(dto.CustomerName))
            {
                var customerDto = new CustomerDto
                {
                    Name = dto.CustomerName,
                    Phone = dto.CustomerPhone,
                    Email = dto.CustomerEmail,
                    DealerId = dto.DealerId
                };

                var (success, message, newCustomer) = await _customerService.CreateAsync(customerDto);
                if (!success || newCustomer == null)
                {
                    throw new ArgumentException($"Không thể tạo khách hàng: {message}");
                }

                dto.CustomerId = newCustomer.Id;
            }

            var testDrive = new TestDrive
            {
                CustomerId = dto.CustomerId,
                VehicleId = dto.VehicleId,
                DealerId = dto.DealerId,
                UserId = dto.UserId,
                ScheduledDate = dto.ScheduledDate,
                StartTime = DateTime.Today.Add(dto.StartTime),
                EndTime = DateTime.Today.Add(dto.EndTime),
                Status = dto.Status == 0 ? 1 : dto.Status, // Default to Pending if not set
                Notes = dto.Notes?.Trim(),
                CustomerName = dto.CustomerName?.Trim(),
                CustomerPhone = dto.CustomerPhone?.Trim(),
                CustomerEmail = dto.CustomerEmail?.Trim()
            };

            return await _testDriveRepo.AddAsync(testDrive);
        }
        catch (Exception ex)
        {
            throw new ApplicationException($"Lỗi khi tạo lịch thử xe: {ex.Message}", ex);
        }
    }

    public async Task<TestDrive> UpdateAsync(TestDriveDto dto)
    {
        try
        {
            var existingTestDrive = await _testDriveRepo.GetByIdAsync(dto.Id);
            if (existingTestDrive == null)
                throw new ArgumentException("Không tìm thấy lịch thử xe");

            existingTestDrive.VehicleId = dto.VehicleId;
            existingTestDrive.CustomerId = dto.CustomerId;
            existingTestDrive.ScheduledDate = dto.ScheduledDate;
            existingTestDrive.StartTime = DateTime.Today.Add(dto.StartTime);
            existingTestDrive.EndTime = DateTime.Today.Add(dto.EndTime);
            existingTestDrive.Status = dto.Status;
            existingTestDrive.Notes = dto.Notes?.Trim();
            existingTestDrive.CustomerName = dto.CustomerName?.Trim();
            existingTestDrive.CustomerPhone = dto.CustomerPhone?.Trim();
            existingTestDrive.CustomerEmail = dto.CustomerEmail?.Trim();

            return await _testDriveRepo.UpdateAsync(existingTestDrive);
        }
        catch (Exception ex)
        {
            throw new ApplicationException($"Lỗi khi cập nhập lịch thử xe: {ex.Message}", ex);
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        return await _testDriveRepo.DeleteAsync(id);
    }

    public async Task<bool> IsTimeSlotAvailableAsync(int vehicleId, DateTime date, TimeSpan startTime, TimeSpan endTime, int? excludeId = null)
    {
        return await _testDriveRepo.IsTimeSlotAvailableAsync(vehicleId, date, startTime, endTime, excludeId);
    }

    public async Task<bool> UpdateStatusAsync(int id, int status)
    {
        var testDrive = await _testDriveRepo.GetByIdAsync(id);
        if (testDrive == null)
            return false;

        testDrive.Status = status;
        await _testDriveRepo.UpdateAsync(testDrive);
        return true;
    }

    public async Task<bool> IsStaffAvailableAsync(int? userId, DateTime date, TimeSpan startTime, TimeSpan endTime, int? excludeId = null)
    {
        return await _testDriveRepo.IsStaffAvailableAsync(userId, date, startTime, endTime, excludeId);
    }

    public async Task<List<TestDriveItemDto>> GetStaffConflictingBookingsAsync(int userId, DateTime date, TimeSpan startTime, TimeSpan endTime, int? excludeId = null)
    {
        var conflictingBookings = await _testDriveRepo.GetStaffConflictingBookingsAsync(userId, date, startTime, endTime, excludeId);
        return conflictingBookings.Select(MapToItemDto).ToList();
    }

    private TestDriveItemDto MapToItemDto(TestDrive testDrive)
    {
        return new TestDriveItemDto
        {
            Id = testDrive.Id,
            CustomerName = testDrive.CustomerName ?? "",
            CustomerPhone = testDrive.CustomerPhone ?? "",
            CustomerEmail = testDrive.CustomerEmail ?? "",
            VehicleName = testDrive.Vehicle?.Model ?? "N/A",
            VehicleModel = testDrive.Vehicle?.Version ?? "",
            ScheduledDate = testDrive.ScheduledDate ?? DateTime.MinValue,
            StartTime = testDrive.StartTime?.TimeOfDay ?? TimeSpan.Zero,
            EndTime = testDrive.EndTime?.TimeOfDay ?? TimeSpan.Zero,
            Status = testDrive.Status ?? 1,
            StatusName = GetStatusName(testDrive.Status ?? 1),
            Notes = testDrive.Notes,
            CreatedAt = testDrive.CreatedAt
        };
    }

    private TestDriveDto MapToDto(TestDrive testDrive)
    {
        return new TestDriveDto
        {
            Id = testDrive.Id,
            VehicleId = testDrive.VehicleId ?? 0,
            CustomerName = testDrive.CustomerName ?? "",
            CustomerPhone = testDrive.CustomerPhone ?? "",
            CustomerEmail = testDrive.CustomerEmail ?? "",
            ScheduledDate = testDrive.ScheduledDate ?? DateTime.Today,
            StartTime = testDrive.StartTime?.TimeOfDay ?? TimeSpan.Zero,
            EndTime = testDrive.EndTime?.TimeOfDay ?? TimeSpan.Zero,
            Notes = testDrive.Notes,
            CustomerId = testDrive.CustomerId,
            DealerId = testDrive.DealerId,
            UserId = testDrive.UserId,
            Status = testDrive.Status ?? 1,
            VehicleName = testDrive.Vehicle?.Model,
            VehicleModel = testDrive.Vehicle?.Version,
            DealerName = testDrive.Dealer?.Name,
            UserName = testDrive.User?.Name,
            StatusName = GetStatusName(testDrive.Status ?? 1),
            CreatedAt = testDrive.CreatedAt
        };
    }

    public string GetStatusName(int status)
    {
        return status switch
        {
            1 => "Cho xac nhan",
            2 => "Da xac nhan",
            3 => "Hoan thanh",
            4 => "Da huy",
            5 => "Khach hang huy",
            6 => "Khong den",
            _ => "Khong xac dinh"
        };
    }
}

