using System.ComponentModel.DataAnnotations;

namespace PRN222.Ass2.EVDealerSys.Models.CustomerManagement;

public class EditCustomerViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Tên khách hàng là bắt buộc")]
    [StringLength(100, ErrorMessage = "Tên không được vượt quá 100 ký tự")]
    [Display(Name = "Tên khách hàng")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
    [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
    [StringLength(20, ErrorMessage = "Số điện thoại không được vượt quá 20 ký tự")]
    [Display(Name = "Số điện thoại")]
    public string Phone { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email là bắt buộc")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    [StringLength(100, ErrorMessage = "Email không được vượt quá 100 ký tự")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Địa chỉ là bắt buộc")]
    [StringLength(200, ErrorMessage = "Địa chỉ không được vượt quá 200 ký tự")]
    [Display(Name = "Địa chỉ")]
    public string Address { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng chọn đại lý")]
    [Display(Name = "Đại lý")]
    public int DealerId { get; set; }

    public string? CurrentDealerName { get; set; }
    public int OrderCount { get; set; }
    public List<DealerSelectItem>? AvailableDealers { get; set; }
}
