using PRN222.Ass2.EVDealerSys.BusinessObjects.Models;
using PRN222.Ass2.EVDealerSys.Repositories.Interfaces;
using PRN222.Ass2.EVDealerSys.Repositories.Context;
using Microsoft.EntityFrameworkCore;
using PRN222.Ass2.EVDealerSys.Repositories.Base;

namespace PRN222.Ass2.EVDealerSys.Repositories.Implementations
{
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        public UserRepository(EvdealerDbContext context) : base(context)
        {
        }

        // Authentication
        public async Task<User?> Auth(string? email, string? pass)
        {
            return await _context.Users
                .AsNoTracking()
                .Include(u => u.Dealer)
                .FirstOrDefaultAsync(u => u.Email == email && u.Password == pass);
        }

        // Get all users
        public override async Task<IEnumerable<User>> GetAllAsync()
        {
            return await _context.Users
                .Include(u => u.Dealer)
                .OrderBy(u => u.Name)
                .ToListAsync();
        }

        // Get user by ID
        public override async Task<User?> GetByIdAsync(int id)
        {
            return await _context.Users
                .Include(u => u.Dealer)
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        // Get user by email
        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Users
                .Include(u => u.Dealer)
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        // Check if email exists (for validation)
        public async Task<bool> EmailExistsAsync(string email, int? excludeUserId = null)
        {
            return await _context.Users
                .AnyAsync(u => u.Email == email && (excludeUserId == null || u.Id != excludeUserId));
        }

        // Get users by role
        public async Task<List<User>> GetByRoleAsync(int role)
        {
            return await _context.Users
                .Include(u => u.Dealer)
                .Where(u => u.Role == role)
                .OrderBy(u => u.Name)
                .ToListAsync();
        }

        // Get users by dealer
        public async Task<List<User>> GetByDealerAsync(int dealerId)
        {
            return await _context.Users
                .Include(u => u.Dealer)
                .Where(u => u.DealerId == dealerId)
                .OrderBy(u => u.Name)
                .ToListAsync();
        }

        // Search users
        public async Task<List<User>> SearchAsync(string searchTerm)
        {
            return await _context.Users
                .Include(u => u.Dealer)
                .Where(u => u.Name!.Contains(searchTerm) || 
                           u.Email!.Contains(searchTerm) ||
                           u.Phone!.Contains(searchTerm))
                .OrderBy(u => u.Name)
                .ToListAsync();
        }
    }
}
