using System.Security.Cryptography;
using System.Text;
using PRN222.Ass2.EVDealerSys.BLL.Interfaces;
using PRN222.Ass2.EVDealerSys.BusinessObjects.Models;
using PRN222.Ass2.EVDealerSys.DAL.Interfaces;

namespace PRN222.Ass2.EVDealerSys.BLL.Implementations;

public class AuthService(IUserRepository userRepo) : IAuthService
{
    private readonly IUserRepository _userRepo = userRepo;

    public async Task<User?> AuthenticateAsync(string email, string password)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            return null;

        var users = await _userRepo.GetAllAsync();
        var user = users.FirstOrDefault(u => u.Email == email);

        if (user == null)
            return null;

        var hashedPassword = HashPassword(password);
        if (user.Password != hashedPassword)
            return null;

        return user;
    }

    public bool IsValidRole(int? role)
    {
        // Hỗ trợ role 1, 2, 3, 4
        return role.HasValue && role >= 1 && role <= 4;
        // 1: Admin, 2: Dealer Manager, 3: Dealer Staff, 4: EVM Staff
    }

    private string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }
}
