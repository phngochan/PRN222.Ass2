using PRN222.Ass2.EVDealerSys.BusinessObjects.Models;
using PRN222.Ass2.EVDealerSys.DAL.Interfaces;
using PRN222.Ass2.EVDealerSys.DAL.Context;
using Microsoft.EntityFrameworkCore;
using PRN222.Ass2.EVDealerSys.DAL.Base;

namespace PRN222.Ass2.EVDealerSys.DAL.Implementations
{
    public class TestDriveRepository : GenericRepository<TestDrive>, ITestDriveRepository
    {
        public TestDriveRepository(EvdealerDbContext context) : base(context)
        {
        }

        public override async Task<IEnumerable<TestDrive>> GetAllAsync()
        {
            return await _context.TestDrives
                .Include(t => t.Customer)
                .Include(t => t.Vehicle)
                .Include(t => t.Dealer)
                .Include(t => t.User)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public override async Task<TestDrive?> GetByIdAsync(int id)
        {
            return await _context.TestDrives
                .Include(t => t.Customer)
                .Include(t => t.Vehicle)
                .Include(t => t.Dealer)
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<List<TestDrive>> GetByDateAsync(DateTime date)
        {
            return await _context.TestDrives
                .Include(t => t.Customer)
                .Include(t => t.Vehicle)
                .Include(t => t.Dealer)
                .Include(t => t.User)
                .Where(t => t.ScheduledDate.HasValue && t.ScheduledDate.Value.Date == date.Date)
                .OrderBy(t => t.StartTime)
                .ToListAsync();
        }

        public async Task<List<TestDrive>> GetByStatusAsync(int status)
        {
            return await _context.TestDrives
                .Include(t => t.Customer)
                .Include(t => t.Vehicle)
                .Include(t => t.Dealer)
                .Include(t => t.User)
                .Where(t => t.Status == status)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<TestDrive>> GetByDealerAsync(int dealerId)
        {
            return await _context.TestDrives
                .Include(t => t.Customer)
                .Include(t => t.Vehicle)
                .Include(t => t.Dealer)
                .Include(t => t.User)
                .Where(t => t.DealerId == dealerId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<TestDrive>> GetUpcomingAsync(int? dealerId = null)
        {
            var query = _context.TestDrives
                .Include(t => t.Customer)
                .Include(t => t.Vehicle)
                .Include(t => t.Dealer)
                .Include(t => t.User)
                .Where(t => t.ScheduledDate.HasValue && t.ScheduledDate.Value >= DateTime.Today);

            if (dealerId.HasValue)
            {
                query = query.Where(t => t.DealerId == dealerId);
            }

            return await query
                .OrderBy(t => t.ScheduledDate)
                .ThenBy(t => t.StartTime)
                .ToListAsync();
        }

        public async Task<TestDrive> AddAsync(TestDrive testDrive)
        {
            testDrive.CreatedAt = DateTime.Now;
            testDrive.UpdatedAt = DateTime.Now;
            
            _context.TestDrives.Add(testDrive);
            await _context.SaveChangesAsync();
            return testDrive;
        }

        public override async Task<TestDrive> UpdateAsync(TestDrive testDrive)
        {
            testDrive.UpdatedAt = DateTime.Now;
            
            _context.TestDrives.Update(testDrive);
            await _context.SaveChangesAsync();
            return testDrive;
        }

        public async Task<bool> IsTimeSlotAvailableAsync(int vehicleId, DateTime date, TimeSpan startTime, TimeSpan endTime, int? excludeId = null)
        {
            // Validate input parameters
            if (startTime >= endTime)
                return false;

            // Check minimum duration (30 minutes)
            if ((endTime - startTime).TotalMinutes < 30)
                return false;

            var query = _context.TestDrives
                .Where(t => t.VehicleId == vehicleId 
                    && t.ScheduledDate.HasValue 
                    && t.ScheduledDate.Value.Date == date.Date
                    && t.Status != 4); // Not cancelled

            if (excludeId.HasValue)
            {
                query = query.Where(t => t.Id != excludeId.Value);
            }

            var existingBookings = await query
                .Where(t => t.StartTime.HasValue && t.EndTime.HasValue)
                .Select(t => new { 
                    Id = t.Id,
                    StartTime = t.StartTime!.Value.TimeOfDay, 
                    EndTime = t.EndTime!.Value.TimeOfDay,
                    Status = t.Status
                })
                .ToListAsync();

            foreach (var booking in existingBookings)
            {
                // Check for overlapping time slots
                // Two time slots overlap if one starts before the other ends
                if (IsTimeSlotOverlap(startTime, endTime, booking.StartTime, booking.EndTime))
                {
                    return false;
                }
            }

            return true;
        }

        private bool IsTimeSlotOverlap(TimeSpan newStart, TimeSpan newEnd, TimeSpan existingStart, TimeSpan existingEnd)
        {
            // Two time slots overlap if:
            // - New slot starts before existing slot ends AND
            // - New slot ends after existing slot starts
            return newStart < existingEnd && newEnd > existingStart;
        }

        public async Task<List<TestDrive>> GetConflictingBookingsAsync(int vehicleId, DateTime date, TimeSpan startTime, TimeSpan endTime, int? excludeId = null)
        {
            var query = _context.TestDrives
                .Include(t => t.Customer)
                .Include(t => t.Vehicle)
                .Where(t => t.VehicleId == vehicleId 
                    && t.ScheduledDate.HasValue 
                    && t.ScheduledDate.Value.Date == date.Date
                    && t.Status != 4 // Not cancelled
                    && t.StartTime.HasValue 
                    && t.EndTime.HasValue);

            if (excludeId.HasValue)
            {
                query = query.Where(t => t.Id != excludeId.Value);
            }

            var bookings = await query.ToListAsync();

            return bookings.Where(booking => 
                IsTimeSlotOverlap(startTime, endTime, 
                    booking.StartTime!.Value.TimeOfDay, 
                    booking.EndTime!.Value.TimeOfDay))
                .ToList();
        }

        public async Task<List<TestDrive>> SearchAsync(string? searchTerm, int? status, DateTime? date, int? vehicleId, int? dealerId = null)
        {
            var query = _context.TestDrives
                .Include(t => t.Customer)
                .Include(t => t.Vehicle)
                .Include(t => t.Dealer)
                .Include(t => t.User)
                .AsQueryable();

            if (dealerId.HasValue)
            {
                query = query.Where(t => t.DealerId == dealerId);
            }

            if (!string.IsNullOrEmpty(searchTerm))
            {
                var term = searchTerm.ToLower();
                query = query.Where(t => 
                    (t.CustomerName != null && t.CustomerName.ToLower().Contains(term)) ||
                    (t.CustomerPhone != null && t.CustomerPhone.ToLower().Contains(term)) ||
                    (t.CustomerEmail != null && t.CustomerEmail.ToLower().Contains(term)));
            }

            if (status.HasValue)
            {
                query = query.Where(t => t.Status == status);
            }

            if (date.HasValue)
            {
                query = query.Where(t => t.ScheduledDate.HasValue && t.ScheduledDate.Value.Date == date.Value.Date);
            }

            if (vehicleId.HasValue)
            {
                query = query.Where(t => t.VehicleId == vehicleId);
            }

            return await query
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> IsStaffAvailableAsync(int? userId, DateTime date, TimeSpan startTime, TimeSpan endTime, int? excludeId = null)
        {
            // If no userId provided, skip staff availability check
            if (!userId.HasValue)
                return true;

            // Validate input parameters
            if (startTime >= endTime)
                return false;

            var query = _context.TestDrives
                .Where(t => t.UserId == userId.Value
                    && t.ScheduledDate.HasValue 
                    && t.ScheduledDate.Value.Date == date.Date
                    && t.Status != 4); // Not cancelled

            if (excludeId.HasValue)
            {
                query = query.Where(t => t.Id != excludeId.Value);
            }

            var existingBookings = await query
                .Where(t => t.StartTime.HasValue && t.EndTime.HasValue)
                .Select(t => new { 
                    Id = t.Id,
                    StartTime = t.StartTime!.Value.TimeOfDay, 
                    EndTime = t.EndTime!.Value.TimeOfDay,
                    Status = t.Status,
                    CustomerName = t.CustomerName
                })
                .ToListAsync();

            foreach (var booking in existingBookings)
            {
                // Check for overlapping time slots
                if (IsTimeSlotOverlap(startTime, endTime, booking.StartTime, booking.EndTime))
                {
                    return false;
                }
            }

            return true;
        }

        public async Task<List<TestDrive>> GetStaffConflictingBookingsAsync(int userId, DateTime date, TimeSpan startTime, TimeSpan endTime, int? excludeId = null)
        {
            var query = _context.TestDrives
                .Include(t => t.Customer)
                .Include(t => t.Vehicle)
                .Where(t => t.UserId == userId
                    && t.ScheduledDate.HasValue 
                    && t.ScheduledDate.Value.Date == date.Date
                    && t.Status != 4 // Not cancelled
                    && t.StartTime.HasValue 
                    && t.EndTime.HasValue);

            if (excludeId.HasValue)
            {
                query = query.Where(t => t.Id != excludeId.Value);
            }

            var bookings = await query.ToListAsync();

            return bookings.Where(booking => 
                IsTimeSlotOverlap(startTime, endTime, 
                    booking.StartTime!.Value.TimeOfDay, 
                    booking.EndTime!.Value.TimeOfDay))
                .ToList();
        }
    }
}