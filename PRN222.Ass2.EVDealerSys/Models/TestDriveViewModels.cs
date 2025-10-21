using System.ComponentModel.DataAnnotations;

namespace PRN222.Ass2.EVDealerSys.Models
{
    public class TestDriveViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn xe")]
        public int VehicleId { get; set; }

        // Remove [Required] validation since these fields are conditionally required
        // Will be validated in controller based on customer selection mode
        [StringLength(255, ErrorMessage = "Tên không được vượt quá 255 ký tự")]
        public string CustomerName { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Số điện thoại không đúng định dạng")]
        [StringLength(20, ErrorMessage = "Số điện thoại không được vượt quá 20 ký tự")]
        public string CustomerPhone { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        [StringLength(255, ErrorMessage = "Email không được vượt quá 255 ký tự")]
        public string CustomerEmail { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn ngày thử xe")]
        [DataType(DataType.Date)]
        public DateTime ScheduledDate { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn giờ bắt đầu")]
        [DataType(DataType.Time)]
        public TimeSpan StartTime { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn giờ kết thúc")]
        [DataType(DataType.Time)]
        public TimeSpan EndTime { get; set; }

        [StringLength(500, ErrorMessage = "Ghi chú không được vượt quá 500 ký tự")]
        public string? Notes { get; set; }


        public int? CustomerId { get; set; }
        public int? DealerId { get; set; }
        public int? UserId { get; set; }
        public int Status { get; set; } = 2; // Default: Confirmed (skip Pending status)

        // Display properties
        public string? VehicleName { get; set; }
        public string? VehicleModel { get; set; }
        public string? DealerName { get; set; }
        public string? UserName { get; set; }
        public string StatusName { get; set; } = string.Empty;
        public DateTime? CreatedAt { get; set; }
    }

    public class TestDriveListViewModel
    {
        public List<TestDriveItemViewModel> TestDrives { get; set; } = new List<TestDriveItemViewModel>();
        public string? SearchTerm { get; set; }
        public int? FilterStatus { get; set; }
        public DateTime? FilterDate { get; set; }
        public int? FilterVehicle { get; set; }

        // Pagination
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class TestDriveItemViewModel
    {
        public int Id { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string VehicleName { get; set; } = string.Empty;
        public string VehicleModel { get; set; } = string.Empty;
        public DateTime ScheduledDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int Status { get; set; }
        public string StatusName { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public DateTime? CreatedAt { get; set; }
    }

    public class TestDriveDashboardViewModel
    {
        public int TodayTestDrivesCount { get; set; }
        public int PendingTestDrives { get; set; }
        public int CompletedTestDrives { get; set; }
        public int TotalTestDrives { get; set; }
        public List<TestDriveItemViewModel> UpcomingTestDrives { get; set; } = new List<TestDriveItemViewModel>();
        public List<TestDriveItemViewModel> TodayTestDrives { get; set; } = new List<TestDriveItemViewModel>();
    }
}
