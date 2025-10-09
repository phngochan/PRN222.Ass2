//using EVDealerSys.BusinessObject.Models;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace EVDealerSys.BLL.Interface;
//public interface IUserService
//{
//    Task<List<User>> GetAllUsersAsync();
//    Task<User?> GetUserByIdAsync(int id);

//    Task<(bool Success, string Message, User? User)> CreateUserAsync(User user);
//    Task<(bool Success, string Message, User? User)> UpdateUserAsync(User user);
//    Task<(bool Success, string Message)> DeleteUserAsync(int id);

//    Task<List<User>> GetUsersByRoleAsync(int role);
//    Task<List<User>> GetUsersByDealerAsync(int dealerId);
//    Task<List<User>> SearchUsersAsync(string searchTerm);

//    string GetRoleName(int? role);
//}
