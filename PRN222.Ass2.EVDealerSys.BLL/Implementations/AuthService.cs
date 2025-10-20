using Microsoft.Extensions.Configuration;

using PRN222.Ass2.EVDealerSys.BLL.Interfaces;
using PRN222.Ass2.EVDealerSys.BusinessObjects.Models;
using PRN222.Ass2.EVDealerSys.DAL.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace PRN222.Ass2.EVDealerSys.BLL.Implementations;

public class AuthService(IConfiguration configuration, IUserRepository userRepo) : IAuthService
{
    private readonly IConfiguration _configuration = configuration;
    private readonly IUserRepository _userRepo = userRepo;

    public async Task<User?> AuthenticateAsync(string email, string password)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            return null;

        // Get user by email
        var users = await _userRepo.GetAllAsync();
        var user = users.FirstOrDefault(u => u.Email == email);
        
        if (user == null)
            return null;

        // Verify password hash
        var hashedPassword = HashPassword(password);
        if (user.Password != hashedPassword)
            return null;

        return user;
    }

    public bool IsValidRole(int? role)
    {
        // Define valid roles (customize based on your requirements)
        return role.HasValue && (role == 1 || role == 2 || role == 3);
        // 1: Admin, 2: Manager, 3: Staff (example)
    }

    // Hash password using SHA256 (same algorithm as UserService)
    private string HashPassword(string password)
    {
        using (var sha256 = SHA256.Create())
        {
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }
    }
}
