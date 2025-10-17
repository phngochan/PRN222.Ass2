using System.ComponentModel.DataAnnotations;

namespace EVDealerSys.Models;

public class VehiclesManagementViewModel
{
    public List<VehicleDto> Vehicles { get; set; } = new List<VehicleDto>();

    public int TotalVehicles { get; set; }
    public int AvailableVehicles { get; set; }
    public int MaintenanceVehicles { get; set; }
    public int SoldVehicles { get; set; }

    // Filters
    public string? SearchModel { get; set; }
    public string? SearchVersion { get; set; }
    public int? FilterStatus { get; set; }
    public string? FilterColor { get; set; }

    public int CurrentPage { get; set; } = 1;
    public int TotalPages { get; set; }
    public int PageSize { get; set; } = 10;
}

public class VehicleDto
{
    public int Id { get; set; }
    public string? Model { get; set; }
    public string? Version { get; set; }
    public string? Color { get; set; }
    public string? Config { get; set; }
    public decimal? Price { get; set; }
    public int? Status { get; set; }
}

public class CreateVehicleViewModel : IValidatableObject
{
    [Required(ErrorMessage = "Model xe là bắt buộc")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Model phải từ 2-100 ký tự")]
    public string VehicleModel { get; set; } = string.Empty;

    [Required(ErrorMessage = "Phiên bản là bắt buộc")]
    [StringLength(50, MinimumLength = 1, ErrorMessage = "Phiên bản phải từ 1-50 ký tự")]
    public string Version { get; set; } = string.Empty;

    [Required(ErrorMessage = "Màu sắc là bắt buộc")]
    public string Color { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Cấu hình không được vượt quá 500 ký tự")]
    public string? Config { get; set; }

    [Required(ErrorMessage = "Giá là bắt buộc")]
    [Range(100000000, 999999999999.99, ErrorMessage = "Giá phải từ 100 triệu đến 999 tỷ VNĐ")]
    public decimal Price { get; set; } = 100000000;

    [Required(ErrorMessage = "Trạng thái là bắt buộc")]
    public int Status { get; set; } = 1;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!string.IsNullOrEmpty(VehicleModel) && VehicleModel.Any(char.IsDigit) && char.IsDigit(VehicleModel[0]))
        {
            yield return new ValidationResult("Model không được bắt đầu bằng số", new[] { nameof(VehicleModel) });
        }
    }
}
#region Details ViewModel
public class VehicleDetailsViewModel
{
    public int Id { get; set; }

    [Display(Name = "Model")]
    public string Model { get; set; } = string.Empty;

    [Display(Name = "Phiên bản")]
    public string Version { get; set; } = string.Empty;

    [Display(Name = "Màu sắc")]
    public string Color { get; set; } = string.Empty;

    [Display(Name = "Cấu hình")]
    public string? Config { get; set; }

    [Display(Name = "Giá (VNĐ)")]
    [DataType(DataType.Currency)]
    public decimal Price { get; set; }

    [Display(Name = "Trạng thái")]
    public int Status { get; set; }

    // Related data counts
    public int TotalInventory { get; set; }
    public int TotalOrders { get; set; }
    public int TotalAllocations { get; set; }
}
#endregion

#region Edit ViewModel
public class EditVehicleViewModel
{
    [Required]
    public int Id { get; set; }

    [Required(ErrorMessage = "Model xe là bắt buộc")]
    [StringLength(100, ErrorMessage = "Model không được vượt quá 100 ký tự")]
    [Display(Name = "Model")]
    public string VehicleModel { get; set; } = string.Empty;

    [Required(ErrorMessage = "Phiên bản là bắt buộc")]
    [StringLength(50, ErrorMessage = "Phiên bản không được vượt quá 50 ký tự")]
    [Display(Name = "Phiên bản")]
    public string Version { get; set; } = string.Empty;

    [Required(ErrorMessage = "Màu sắc là bắt buộc")]
    [StringLength(30, ErrorMessage = "Màu sắc không được vượt quá 30 ký tự")]
    [Display(Name = "Màu sắc")]
    public string Color { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Cấu hình không được vượt quá 500 ký tự")]
    [Display(Name = "Cấu hình")]
    public string? Config { get; set; }

    [Required(ErrorMessage = "Giá là bắt buộc")]
    [Range(0, 999999999999.99, ErrorMessage = "Giá phải từ 0 đến 999,999,999,999.99")]
    [Display(Name = "Giá (VNĐ)")]
    [DataType(DataType.Currency)]
    public decimal Price { get; set; }

    [Required(ErrorMessage = "Trạng thái là bắt buộc")]
    [Display(Name = "Trạng thái")]
    public int Status { get; set; }

    // Additional info for display
    public int InventoryCount { get; set; }
    public int OrderCount { get; set; }
    public int AllocationCount { get; set; }
}
#endregion

#region delete ViewModel
public class DeleteVehicleViewModel
{
    public int Id { get; set; }

    [Display(Name = "Model")]
    public string VehicleModel { get; set; } = string.Empty;

    [Display(Name = "Phiên bản")]
    public string Version { get; set; } = string.Empty;

    [Display(Name = "Màu sắc")]
    public string Color { get; set; } = string.Empty;

    [Display(Name = "Cấu hình")]
    public string? Config { get; set; }

    [Display(Name = "Giá (VNĐ)")]
    [DataType(DataType.Currency)]
    public decimal Price { get; set; }

    [Display(Name = "Trạng thái")]
    public int Status { get; set; }

    public string StatusText => Status switch
    {
        1 => "Có sẵn",
        2 => "Đã bán",
        3 => "Bảo trì",
        4 => "Đã đặt trước",
        _ => "Không xác định"
    };

    // Warning information
    public int TotalInventory { get; set; }
    public int TotalOrders { get; set; }
    public int TotalAllocations { get; set; }

    public bool HasRelatedData => TotalInventory > 0 || TotalOrders > 0 || TotalAllocations > 0;

    public string GetWarningMessage()
    {
        if (!HasRelatedData) return string.Empty;

        var warnings = new List<string>();
        if (TotalInventory > 0) warnings.Add($"{TotalInventory} bản ghi tồn kho");
        if (TotalOrders > 0) warnings.Add($"{TotalOrders} đơn hàng");
        if (TotalAllocations > 0) warnings.Add($"{TotalAllocations} phân bổ");

        return $"Xe này có liên quan đến {string.Join(", ", warnings)}. Xóa xe sẽ ảnh hưởng đến các bản ghi này.";
    }
}
#endregion