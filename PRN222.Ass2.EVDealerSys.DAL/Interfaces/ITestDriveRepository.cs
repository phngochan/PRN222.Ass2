using PRN222.Ass2.EVDealerSys.BusinessObjects.Models;
using PRN222.Ass2.EVDealerSys.DAL.Base;

namespace PRN222.Ass2.EVDealerSys.DAL.Interfaces
{
    public interface ITestDriveRepository : IGenericRepository<TestDrive>
    {
        // CRUD Operations
        Task<TestDrive> AddAsync(TestDrive testDrive);

        // Query Methods
        Task<List<TestDrive>> GetByDateAsync(DateTime date);
        Task<List<TestDrive>> GetByStatusAsync(int status);
        Task<List<TestDrive>> GetByDealerAsync(int dealerId);
        Task<List<TestDrive>> GetUpcomingAsync(int? dealerId = null);

        // Validation
        Task<bool> IsTimeSlotAvailableAsync(int vehicleId, DateTime date, TimeSpan startTime, TimeSpan endTime, int? excludeId = null);
        Task<bool> IsStaffAvailableAsync(int? userId, DateTime date, TimeSpan startTime, TimeSpan endTime, int? excludeId = null);

        // Search & Conflict Detection
        Task<List<TestDrive>> SearchAsync(string? searchTerm, int? status, DateTime? date, int? vehicleId, int? dealerId = null);
        Task<List<TestDrive>> GetConflictingBookingsAsync(int vehicleId, DateTime date, TimeSpan startTime, TimeSpan endTime, int? excludeId = null);
        Task<List<TestDrive>> GetStaffConflictingBookingsAsync(int userId, DateTime date, TimeSpan startTime, TimeSpan endTime, int? excludeId = null);
    }
}
