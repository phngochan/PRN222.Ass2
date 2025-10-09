using Microsoft.Extensions.Configuration;
using PRN222.Ass2.EVDealerSys.BusinessObjects.Models;
using PRN222.Ass2.EVDealerSys.Repositories.Interfaces;
using PRN222.Ass2.EVDealerSys.Services.Interfaces;

namespace PRN222.Ass2.EVDealerSys.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly IConfiguration _configuration;
        private readonly IUserRepository _userRepo;

        public AuthService(IConfiguration configuration, IUserRepository userRepo)
        {
            _configuration = configuration;
            _userRepo = userRepo;
        }

        public async Task<User?> AuthenticateAsync(string email, string password)
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                return null;

            // Hash password if needed (currently using plain text)
            // string hashedPassword = HashPassword(password);

            // Authenticate user
            var user = await _userRepo.Auth(email, password);
            return user;
        }

        public bool IsValidRole(int? role)
        {
            // Define valid roles (customize based on your requirements)
            return role.HasValue && (role == 1 || role == 2 || role == 3);
            // 1: Admin, 2: Manager, 3: Staff (example)
        }

        // Future: Add password hashing
        private string HashPassword(string password)
        {
            // Implement password hashing (BCrypt, etc.)
            return password; // Temporary
        }
    }
}