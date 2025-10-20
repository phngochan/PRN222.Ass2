using System.ComponentModel.DataAnnotations;

namespace PRN222.Ass2.EVDealerSys.Models;

public class DealerManagementViewModel
{
    public List<DealerListItem> Dealers { get; set; } = new List<DealerListItem>();
    public int TotalDealers { get; set; }
    public int TotalCustomers { get; set; }
    public int TotalOrders { get; set; }
    public int TotalRegions { get; set; }
    public int CurrentPage { get; set; } = 1;
    public int TotalPages { get; set; }
    public int TotalCount { get; set; }
    public string? SearchName { get; set; }
    public string? SearchAddress { get; set; }
    public string? SelectedRegion { get; set; }
    public List<string> AvailableRegions { get; set; } = new List<string>();
}

public class DealerListItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public int CustomerCount { get; set; }
    public int InventoryCount { get; set; }
    public int OrderCount { get; set; }
}
public class CreateDealerViewModel
{
    [Required(ErrorMessage = "Tên đại lý là bắt buộc")]
    [StringLength(100, ErrorMessage = "Tên không được vượt quá 100 ký tự")]
    [Display(Name = "Tên đại lý")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Địa chỉ là bắt buộc")]
    [StringLength(200, ErrorMessage = "Địa chỉ không được vượt quá 200 ký tự")]
    [Display(Name = "Địa chỉ")]
    public string Address { get; set; } = string.Empty;

    [Required(ErrorMessage = "Khu vực là bắt buộc")]
    [StringLength(50, ErrorMessage = "Khu vực không được vượt quá 50 ký tự")]
    [Display(Name = "Khu vực")]
    public string Region { get; set; } = string.Empty;
}

#region detail view model
public class DealerDetailsViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public int TotalCustomers { get; set; }
    public int TotalInventories { get; set; }
    public int TotalOrders { get; set; }
    public int TotalUsers { get; set; }
    public int TotalAllocations { get; set; }
    public decimal TotalRevenue { get; set; }
    public List<DealerCustomerInfo> TopCustomers { get; set; } = new List<DealerCustomerInfo>();
    public List<DealerInventoryInfo> RecentInventories { get; set; } = new List<DealerInventoryInfo>();
}

public class DealerCustomerInfo
{
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int OrderCount { get; set; }
}

public class DealerInventoryInfo
{
    public int InventoryId { get; set; }
    public string VehicleModel { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public DateTime LastUpdated { get; set; }
}
#endregion
#region Edit view model
public class EditDealerViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Tên đại lý là bắt buộc")]
    [StringLength(100, ErrorMessage = "Tên không được vượt quá 100 ký tự")]
    [Display(Name = "Tên đại lý")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Địa chỉ là bắt buộc")]
    [StringLength(200, ErrorMessage = "Địa chỉ không được vượt quá 200 ký tự")]
    [Display(Name = "Địa chỉ")]
    public string Address { get; set; } = string.Empty;

    [Required(ErrorMessage = "Khu vực là bắt buộc")]
    [StringLength(50, ErrorMessage = "Khu vực không được vượt quá 50 ký tự")]
    [Display(Name = "Khu vực")]
    public string Region { get; set; } = string.Empty;

    public int CustomerCount { get; set; }
    public int InventoryCount { get; set; }
    public int OrderCount { get; set; }
    public int UserCount { get; set; }
}
#endregion
#region Delete view model
public class DeleteDealerViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public int CustomerCount { get; set; }
    public int InventoryCount { get; set; }
    public int OrderCount { get; set; }
    public int UserCount { get; set; }
    public int AllocationCount { get; set; }

    public bool HasRelatedData => CustomerCount > 0 || InventoryCount > 0 || OrderCount > 0 || UserCount > 0 || AllocationCount > 0;

    public string GetWarningMessage()
    {
        var warnings = new List<string>();

        if (CustomerCount > 0) warnings.Add($"{CustomerCount} khách hàng");
        if (InventoryCount > 0) warnings.Add($"{InventoryCount} kho hàng");
        if (OrderCount > 0) warnings.Add($"{OrderCount} đơn hàng");
        if (UserCount > 0) warnings.Add($"{UserCount} người dùng");
        if (AllocationCount > 0) warnings.Add($"{AllocationCount} phân bổ xe");

        if (warnings.Any())
        {
            return $"Đại lý này có {string.Join(", ", warnings)}. Xóa đại lý sẽ ảnh hưởng đến tất cả dữ liệu liên quan.";
        }

        return string.Empty;
    }
}
#endregion
