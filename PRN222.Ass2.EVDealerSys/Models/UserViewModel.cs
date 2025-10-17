using System.ComponentModel.DataAnnotations;

namespace PRN222.Ass2.EVDealerSys.Models
{
    public class UserViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên là bắt buộc")]
        [StringLength(255, ErrorMessage = "Tên không được vượt quá 255 ký tự")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        [StringLength(255, ErrorMessage = "Email không được vượt quá 255 ký tự")]
        public string Email { get; set; } = string.Empty;

        [StringLength(20, ErrorMessage = "Số điện thoại không được vượt quá 20 ký tự")]
        public string? Phone { get; set; }

        [Required(ErrorMessage = "Role là bắt buộc")]
        [Range(1, 3, ErrorMessage = "Role phải từ 1 đến 3")]
        public int Role { get; set; }

        public int? DealerId { get; set; }

        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải có từ 6 đến 100 ký tự")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Mật khẩu xác nhận không khớp")]
        public string ConfirmPassword { get; set; } = string.Empty;

        // Display properties
        public string? DealerName { get; set; }
        public string RoleName { get; set; } = string.Empty;
    }

    public class UserEditViewModel
    {
        public int Id { get; set; }

        [StringLength(255, ErrorMessage = "Tên không được vượt quá 255 ký tự")]
        public string Name { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        [StringLength(255, ErrorMessage = "Email không được vượt quá 255 ký tự")]
        public string Email { get; set; } = string.Empty;

        [StringLength(20, ErrorMessage = "Số điện thoại không được vượt quá 20 ký tự")]
        public string? Phone { get; set; }

        [Required(ErrorMessage = "Role là bắt buộc")]
        [Range(1, 3, ErrorMessage = "Role phải từ 1 đến 3")]
        public int Role { get; set; }

        public int? DealerId { get; set; }

        [DataType(DataType.Password)]
        public string? Password { get; set; }

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Mật khẩu xác nhận không khớp")]
        public string? ConfirmPassword { get; set; }

        // Display properties
        public string? DealerName { get; set; }
        public string RoleName { get; set; } = string.Empty;
    }

    public class UserListViewModel
    {
        public List<UserItemViewModel> Users { get; set; } = new List<UserItemViewModel>();
        public string? SearchTerm { get; set; }
        public int? FilterRole { get; set; }
        public int? FilterDealer { get; set; }

        // Pagination (for future implementation)
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class UserItemViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public int Role { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public int? DealerId { get; set; }
        public string? DealerName { get; set; }
    }
}
