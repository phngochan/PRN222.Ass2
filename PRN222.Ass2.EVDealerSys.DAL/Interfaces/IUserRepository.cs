using PRN222.Ass2.EVDealerSys.BusinessObjects.Models;
using PRN222.Ass2.EVDealerSys.DAL.Base;

namespace PRN222.Ass2.EVDealerSys.DAL.Interfaces
{
    public interface IUserRepository : IGenericRepository<User>
    {
        // Authentication
        Task<User?> Auth(string? email, string? pass);

        // CRUD Operations
        Task<User?> GetByEmailAsync(string email);

        // Validation
        Task<bool> EmailExistsAsync(string email, int? excludeUserId = null);

        // Query Methods
        Task<List<User>> GetByRoleAsync(int role);
        Task<List<User>> GetByDealerAsync(int dealerId);
        Task<List<User>> SearchAsync(string searchTerm);
    }
}
