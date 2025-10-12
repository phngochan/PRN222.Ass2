using PRN222.Ass2.EVDealerSys.BusinessObjects.Models;

namespace PRN222.Ass2.EVDealerSys.BLL.Interfaces;
public interface IUserService
{
    Task<IEnumerable<User>> GetAllUsersAsync();
    Task<User?> GetUserByIdAsync(int id);

    Task<(bool Success, string Message, User? User)> CreateUserAsync(User user);
    Task<(bool Success, string Message, User? User)> UpdateUserAsync(User user);
    Task<(bool Success, string Message)> DeleteUserAsync(int id);

    Task<List<User>> GetUsersByRoleAsync(int role);
    Task<List<User>> GetUsersByDealerAsync(int dealerId);
    Task<IEnumerable<User>> SearchUsersAsync(string searchTerm);

    string GetRoleName(int? role);
}
