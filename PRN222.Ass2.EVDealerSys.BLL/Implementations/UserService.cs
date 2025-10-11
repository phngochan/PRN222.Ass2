//using EVDealerSys.BLL.Interface;
//using EVDealerSys.BusinessObject.Models;
//using EVDealerSys.DAL.Interfaces;

//namespace EVDealerSys.BLL.Services
//{
//    public class UserService: IUserService
//    {
//        private readonly IUserRepository _userRepo;

//        public UserService(IUserRepository userRepo)
//        {
//            _userRepo = userRepo;
//        }

//        // Get all users
//        public async Task<List<User>> GetAllUsersAsync()
//        {
//            return await _userRepo.GetAllAsync();
//        }

//        // Get user by ID
//        public async Task<User?> GetUserByIdAsync(int id)
//        {
//            return await _userRepo.GetByIdAsync(id);
//        }

//        // Create new user
//        public async Task<(bool Success, string Message, User? User)> CreateUserAsync(User user)
//        {
//            try
//            {
//                // Validate input
//                var validationResult = ValidateUser(user);
//                if (!validationResult.IsValid)
//                {
//                    return (false, validationResult.Message, null);
//                }

//                // Check if email already exists
//                if (await _userRepo.EmailExistsAsync(user.Email!))
//                {
//                    return (false, "Email đã được sử dụng bởi user khác!", null);
//                }

//                // Hash password (in future implementation)
//                // user.Password = HashPassword(user.Password);

//                var createdUser = await _userRepo.CreateAsync(user);
//                return (true, "Tạo user thành công!", createdUser);
//            }
//            catch (Exception ex)
//            {
//                return (false, $"Có lỗi xảy ra: {ex.Message}", null);
//            }
//        }

//        // Update user
//        public async Task<(bool Success, string Message, User? User)> UpdateUserAsync(User user)
//        {
//            try
//            {
//                // Check if user exists
//                var existingUser = await _userRepo.GetByIdAsync(user.Id);
//                if (existingUser == null)
//                {
//                    return (false, "User không tồn tại!", null);
//                }

//                // Validate input
//                var validationResult = ValidateUser(user);
//                if (!validationResult.IsValid)
//                {
//                    return (false, validationResult.Message, null);
//                }

//                // Check if email already exists (exclude current user)
//                if (await _userRepo.EmailExistsAsync(user.Email!, user.Id))
//                {
//                    return (false, "Email đã được sử dụng bởi user khác!", null);
//                }

//                // Keep existing password if not provided
//                if (string.IsNullOrWhiteSpace(user.Password))
//                {
//                    user.Password = existingUser.Password;
//                }
//                else
//                {
//                    // Hash new password (in future implementation)
//                    // user.Password = HashPassword(user.Password);
//                }

//                var updatedUser = await _userRepo.UpdateAsync(user);
//                return (true, "Cập nhật user thành công!", updatedUser);
//            }
//            catch (Exception ex)
//            {
//                return (false, $"Có lỗi xảy ra: {ex.Message}", null);
//            }
//        }

//        // Delete user
//        public async Task<(bool Success, string Message)> DeleteUserAsync(int id)
//        {
//            try
//            {
//                // Check if user exists
//                var user = await _userRepo.GetByIdAsync(id);
//                if (user == null)
//                {
//                    return (false, "User không tồn tại!");
//                }

//                // Check if user has orders (business rule)
//                // You might want to prevent deletion if user has active orders
//                // This would require additional logic

//                var success = await _userRepo.DeleteAsync(id);
//                if (success)
//                {
//                    return (true, "Xóa user thành công!");
//                }
//                else
//                {
//                    return (false, "Không thể xóa user!");
//                }
//            }
//            catch (Exception ex)
//            {
//                return (false, $"Có lỗi xảy ra: {ex.Message}");
//            }
//        }

//        // Get users by role
//        public async Task<List<User>> GetUsersByRoleAsync(int role)
//        {
//            return await _userRepo.GetByRoleAsync(role);
//        }

//        // Get users by dealer
//        public async Task<List<User>> GetUsersByDealerAsync(int dealerId)
//        {
//            return await _userRepo.GetByDealerAsync(dealerId);
//        }

//        // Search users
//        public async Task<List<User>> SearchUsersAsync(string searchTerm)
//        {
//            if (string.IsNullOrWhiteSpace(searchTerm))
//            {
//                return await GetAllUsersAsync();
//            }

//            return await _userRepo.SearchAsync(searchTerm);
//        }

//        // Validate user data
//        private (bool IsValid, string Message) ValidateUser(User user)
//        {
//            if (string.IsNullOrWhiteSpace(user.Name))
//            {
//                return (false, "Tên user là bắt buộc!");
//            }

//            if (string.IsNullOrWhiteSpace(user.Email))
//            {
//                return (false, "Email là bắt buộc!");
//            }

//            if (!IsValidEmail(user.Email))
//            {
//                return (false, "Email không đúng định dạng!");
//            }

//            if (string.IsNullOrWhiteSpace(user.Password))
//            {
//                return (false, "Mật khẩu là bắt buộc!");
//            }

//            if (user.Password.Length < 6)
//            {
//                return (false, "Mật khẩu phải có ít nhất 6 ký tự!");
//            }

//            if (user.Role == null || !IsValidRole(user.Role.Value))
//            {
//                return (false, "Role không hợp lệ!");
//            }

//            return (true, string.Empty);
//        }

//        // Validate email format
//        private bool IsValidEmail(string email)
//        {
//            try
//            {
//                var addr = new System.Net.Mail.MailAddress(email);
//                return addr.Address == email;
//            }
//            catch
//            {
//                return false;
//            }
//        }

//        // Validate role
//        private bool IsValidRole(int role)
//        {
//            // 1: Admin, 2: Manager, 3: Staff
//            return role >= 1 && role <= 3;
//        }

//        // Get role name (for display)
//        public string GetRoleName(int? role)
//        {
//            return role switch
//            {
//                1 => "Admin",
//                2 => "Manager", 
//                3 => "Staff",
//                _ => "Unknown"
//            };
//        }

//        // Future: Add password hashing
//        private string HashPassword(string password)
//        {
//            // Implement password hashing (BCrypt, etc.)
//            return password; // Temporary
//        }
//    }
//}