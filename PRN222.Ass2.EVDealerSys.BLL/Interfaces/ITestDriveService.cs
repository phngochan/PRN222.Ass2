using PRN222.Ass2.EVDealerSys.BusinessObjects.DTO.TestDrive;
using PRN222.Ass2.EVDealerSys.BusinessObjects.Models;

namespace PRN222.Ass2.EVDealerSys.BLL.Interfaces;
public interface ITestDriveService
{
    Task<List<TestDriveItemDto>> GetAllAsync(int? dealerId = null);
    Task<TestDriveDto?> GetByIdAsync(int id);
    Task<TestDriveDashboardDto> GetDashboardDataAsync(int? dealerId = null);
    Task<List<TestDriveItemDto>> GetByDateAsync(DateTime date, int? dealerId = null);
    Task<List<TestDriveItemDto>> SearchAsync(string? searchTerm, int? status, DateTime? date, int? vehicleId, int? dealerId = null);
    Task<TestDrive> CreateAsync(TestDriveDto dto);
    Task<TestDrive> UpdateAsync(TestDriveDto dto);
    Task<bool> DeleteAsync(int id);
    Task<bool> IsTimeSlotAvailableAsync(int vehicleId, DateTime date, TimeSpan startTime, TimeSpan endTime, int? excludeId = null);
    Task<bool> UpdateStatusAsync(int id, int status);
    Task<bool> IsStaffAvailableAsync(int? userId, DateTime date, TimeSpan startTime, TimeSpan endTime, int? excludeId = null);
    Task<List<TestDriveItemDto>> GetStaffConflictingBookingsAsync(int userId, DateTime date, TimeSpan startTime, TimeSpan endTime, int? excludeId = null);
}

