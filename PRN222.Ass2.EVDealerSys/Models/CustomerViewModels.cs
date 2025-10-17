using System.ComponentModel.DataAnnotations;

namespace PRN222.Ass2.EVDealerSys.Models;

public class CustomerViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Tên khách hàng là bắt buộc")]
    [StringLength(255, ErrorMessage = "Tên không được vượt quá 255 ký tự")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
    [Phone(ErrorMessage = "Số điện thoại không đúng định dạng")]
    [StringLength(20, ErrorMessage = "Số điện thoại không được vượt quá 20 ký tự")]
    public string Phone { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email là bắt buộc")]
    [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
    [StringLength(255, ErrorMessage = "Email không được vượt quá 255 ký tự")]
    public string Email { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Địa chỉ không được vượt quá 500 ký tự")]
    public string? Address { get; set; }

    public int? DealerId { get; set; }
    public string? DealerName { get; set; }
}

public class CustomerItemViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Address { get; set; }
    public int? DealerId { get; set; }
    public string? DealerName { get; set; }
    public string DisplayText => $"{Name} - {Phone}";
}

public class CustomerSearchViewModel
{
    public List<CustomerItemViewModel> Customers { get; set; } = new List<CustomerItemViewModel>();
    public string? SearchTerm { get; set; }
}
