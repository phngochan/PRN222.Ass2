using System.Globalization;
using System.Security.Claims;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.SignalR;

using PRN222.Ass2.EVDealerSys.Base.BasePageModels;
using PRN222.Ass2.EVDealerSys.BLL.Interfaces;
using PRN222.Ass2.EVDealerSys.BusinessObjects.DTO.TestDrive;
using PRN222.Ass2.EVDealerSys.Hubs;
using PRN222.Ass2.EVDealerSys.Models;

namespace PRN222.Ass2.EVDealerSys.Pages.TestDrives;

public class CreateModel : BaseCrudPageModel
{
    private readonly ITestDriveService _testDriveService;
    private readonly IVehicleService _vehicleService;
    private readonly ICustomerService _customerService;
    private readonly IHubContext<TestDriveHub> _hubContext;
    private readonly ILogger<CreateModel> _logger;

    public CreateModel(IActivityLogService logService,
        ITestDriveService testDriveService,
        IVehicleService vehicleService,
        ICustomerService customerService,
        ILogger<CreateModel> logger, 
        IHubContext<ActivityLogHub> activityLogHubContext,
        IHubContext<TestDriveHub> hubContext) : base(logService)
    {
        _testDriveService = testDriveService;
        _vehicleService = vehicleService;
        _customerService = customerService;
        _hubContext = hubContext;
        _logger = logger;

        SetActivityLogHubContext(activityLogHubContext);
    }

    [BindProperty]
    public TestDriveViewModel Form { get; set; } = new();

    public List<SelectListItem> VehicleOptions { get; private set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        await LoadVehiclesAsync();
        EnsureDefaultValues();
        await LogAsync("Open Create Test Drive", "User opened test drive creation form");
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await LoadVehiclesAsync();

        if (!ModelState.IsValid)
        {
            return Page();
        }

        if (Form.VehicleId <= 0)
        {
            ModelState.AddModelError(nameof(Form.VehicleId), "Vui lòng chọn xe để thử.");
            return Page();
        }

        if (Form.StartTime >= Form.EndTime)
        {
            ModelState.AddModelError(nameof(Form.EndTime), "Giờ kết thúc phải sau giờ bắt đầu.");
            return Page();
        }

        if ((Form.EndTime - Form.StartTime).TotalMinutes < 30)
        {
            ModelState.AddModelError(nameof(Form.EndTime), "Thời gian thử xe tối thiểu là 30 phút.");
            return Page();
        }

        // Kiểm tra thời gian tối đa (2 giờ)
        if ((Form.EndTime - Form.StartTime).TotalHours > 2)
        {
            ModelState.AddModelError(nameof(Form.EndTime), "Thời gian thử xe tối đa là 2 giờ.");
            return Page();
        }

        // Kiểm tra giờ làm việc (8:00 - 18:00)
        var workingHoursStart = new TimeSpan(8, 0, 0);
        var workingHoursEnd = new TimeSpan(18, 0, 0);

        if (Form.StartTime < workingHoursStart || Form.EndTime > workingHoursEnd)
        {
            ModelState.AddModelError(nameof(Form.StartTime), "Lịch thử xe chỉ có thể đặt trong giờ làm việc (8:00 - 18:00).");
            return Page();
        }

        // Kiểm tra ngày không được là quá khứ
        if (Form.ScheduledDate.Date < DateTime.Today)
        {
            ModelState.AddModelError(nameof(Form.ScheduledDate), "Ngày thử xe không được là ngày trong quá khứ.");
            return Page();
        }

        // Nếu đặt lịch hôm nay, kiểm tra giờ phải sau giờ hiện tại
        if (Form.ScheduledDate.Date == DateTime.Today && Form.StartTime <= DateTime.Now.TimeOfDay)
        {
            ModelState.AddModelError(nameof(Form.StartTime), "Giờ bắt đầu phải sau giờ hiện tại.");
            return Page();
        }

        if (!Form.CustomerId.HasValue &&
            (string.IsNullOrWhiteSpace(Form.CustomerName) ||
             string.IsNullOrWhiteSpace(Form.CustomerPhone) ||
             string.IsNullOrWhiteSpace(Form.CustomerEmail)))
        {
            ModelState.AddModelError(nameof(Form.CustomerName), "Vui lòng cung cấp đầy đủ thông tin khách hàng.");
            return Page();
        }

        var dealerId = GetDealerId();
        var userId = GetUserId();

        var dto = new TestDriveDto
        {
            VehicleId = Form.VehicleId,
            CustomerId = Form.CustomerId,
            CustomerName = Form.CustomerName?.Trim() ?? string.Empty,
            CustomerPhone = Form.CustomerPhone?.Trim() ?? string.Empty,
            CustomerEmail = Form.CustomerEmail?.Trim() ?? string.Empty,
            ScheduledDate = Form.ScheduledDate,
            StartTime = Form.StartTime,
            EndTime = Form.EndTime,
            Notes = string.IsNullOrWhiteSpace(Form.Notes) ? null : Form.Notes.Trim(),
            DealerId = dealerId,
            UserId = userId,
            Status = Form.Status == 0 ? 2 : Form.Status // Default to Confirmed (2)
        };

        try
        {
            var createdTestDrive = await _testDriveService.CreateAsync(dto);
            
            // Send real-time notification via SignalR
            await _hubContext.Clients.All.SendAsync("TestDriveCreated", new
            {
                id = createdTestDrive.Id,
                vehicleName = Form.VehicleName,
                customerName = Form.CustomerName,
                scheduledDate = Form.ScheduledDate.ToString("dd/MM/yyyy"),
                startTime = Form.StartTime.ToString(@"hh\:mm"),
                endTime = Form.EndTime.ToString(@"hh\:mm"),
                status = createdTestDrive.Status,
                statusName = GetStatusName(createdTestDrive.Status ?? 2),
                timestamp = DateTime.Now
            });
            
            _logger.LogInformation("Test drive created and SignalR notification sent: {Id}", createdTestDrive.Id);
            
            TempData["SuccessMessage"] = "Đặt lịch thử xe thành công.";

            return RedirectToPage("./Index");
        }
        catch (ApplicationException ex)
        {
            _logger.LogWarning(ex, "Invalid data while creating test drive booking");
            await LogAsync("Error", $"Invalid test drive data: {ex.Message}");
            ModelState.AddModelError(string.Empty, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error when creating test drive booking");
            await LogAsync("Error", $"Failed to create test drive: {ex.Message}");
            ModelState.AddModelError(string.Empty, "Có lỗi xảy ra khi đặt lịch thử xe.");
        }

        return Page();
    }

    public async Task<JsonResult> OnPostCheckPhoneAsync(string phone, int? excludeId)
    {
        if (string.IsNullOrWhiteSpace(phone))
        {
            return new JsonResult(new { exists = false });
        }

        var exists = await _customerService.IsPhoneExistsAsync(phone.Trim(), excludeId);
        return new JsonResult(new { exists });
    }

    public async Task<JsonResult> OnPostCheckEmailAsync(string email, int? excludeId)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return new JsonResult(new { exists = false });
        }

        var exists = await _customerService.IsEmailExistsAsync(email.Trim(), excludeId);
        return new JsonResult(new { exists });
    }

    public async Task<JsonResult> OnPostSearchCustomersAsync(string searchTerm)
    {
        try
        {
            var dealerId = GetDealerId();
            var customers = await _customerService.SearchAsync(searchTerm, dealerId);

            var result = customers.Select(c => new
            {
                id = c.Id,
                name = c.Name,
                phone = c.Phone,
                email = c.Email,
                address = c.Address
            });

            return new JsonResult(new { success = true, customers = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching customers with term {Term}", searchTerm);
            return new JsonResult(new { success = false, error = "Không thể tìm kiếm khách hàng." });
        }
    }

    public async Task<JsonResult> OnPostCheckTimeSlotAvailabilityAsync(int vehicleId, DateTime date, string startTime, string endTime)
    {
        if (!TimeSpan.TryParse(startTime, out var start) || !TimeSpan.TryParse(endTime, out var end))
        {
            return new JsonResult(new { available = false, error = "Giờ không hợp lệ." });
        }

        var available = await _testDriveService.IsTimeSlotAvailableAsync(vehicleId, date, start, end);
        return available
            ? new JsonResult(new { available = true })
            : new JsonResult(new { available = false, error = "Xe đã được đặt trong khung giờ này." });
    }

    public async Task<JsonResult> OnPostCheckStaffAvailabilityAsync(DateTime date, string startTime, string endTime)
    {
        var userId = GetUserId();
        if (!userId.HasValue)
        {
            return new JsonResult(new { available = true });
        }

        if (!TimeSpan.TryParse(startTime, out var start) || !TimeSpan.TryParse(endTime, out var end))
        {
            return new JsonResult(new { available = false, error = "Giờ không hợp lệ." });
        }

        var available = await _testDriveService.IsStaffAvailableAsync(userId.Value, date, start, end);
        return available
            ? new JsonResult(new { available = true })
            : new JsonResult(new { available = false, error = "Bạn đã có lịch khác trong khung giờ này." });
    }

    public async Task<JsonResult> OnPostGetStaffScheduleAsync(DateTime date)
    {
        var userId = GetUserId();
        if (!userId.HasValue)
        {
            return new JsonResult(new { success = false, error = "Không xác định được nhân viên." });
        }

        try
        {
            var dealerId = GetDealerId();
            var bookings = await _testDriveService.GetByDateAsync(date, dealerId);

            var schedule = bookings
                .Where(b => b.UserId == userId.Value)
                .OrderBy(b => b.StartTime)
                .Select(b => new
                {
                    startTime = b.StartTime.ToString(@"hh\:mm"),
                    endTime = b.EndTime.ToString(@"hh\:mm"),
                    customerName = b.CustomerName,
                    vehicleName = string.IsNullOrWhiteSpace(b.VehicleName) ? "N/A" : b.VehicleName,
                    status = b.StatusName
                })
                .ToList();

            return new JsonResult(new { success = true, schedule });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching staff schedule for user {UserId} on {Date}", userId, date);
            return new JsonResult(new { success = false, error = "Không thể tải lịch trình." });
        }
    }

    private async Task LoadVehiclesAsync()
    {
        var vehicles = await _vehicleService.GetAllVehiclesActiveAsync();

        VehicleOptions = vehicles
            .OrderBy(v => v.Model)
            .Select(v => new SelectListItem
            {
                Value = v.Id.ToString(CultureInfo.InvariantCulture),
                Text = string.IsNullOrWhiteSpace(v.Version) ? v.Model ?? "Xe không tên" : $"{v.Model} - {v.Version}"
            })
            .ToList();
    }

    private void EnsureDefaultValues()
    {
        if (Form.ScheduledDate == default)
        {
            Form.ScheduledDate = DateTime.Today.AddDays(1);
        }

        if (Form.StartTime == default)
        {
            // Nếu đặt lịch cho hôm nay, set giờ ít nhất 1 tiếng sau giờ hiện tại
            if (Form.ScheduledDate.Date == DateTime.Today)
            {
                var nextHour = DateTime.Now.AddHours(1);
                Form.StartTime = new TimeSpan(nextHour.Hour, 0, 0);

                // Đảm bảo không vượt quá giờ làm việc
                if (Form.StartTime.Hours >= 18)
                {
                    Form.ScheduledDate = DateTime.Today.AddDays(1);
                    Form.StartTime = new TimeSpan(9, 0, 0);
                }
            }
            else
            {
                Form.StartTime = new TimeSpan(9, 0, 0);
            }
        }

        if (Form.EndTime == default)
        {
            Form.EndTime = Form.StartTime.Add(new TimeSpan(1, 0, 0)); // Default 1 giờ
        }
    }

    private int? GetDealerId()
    {
        var dealerClaim = User.FindFirst("DealerId")?.Value;
        return int.TryParse(dealerClaim, out var dealerId) ? dealerId : null;
    }

    private int? GetUserId()
    {
        var userClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userClaim, out var userId) ? userId : null;
    }

    private static string GetStatusName(int status) => status switch
    {
        1 => "Chờ xác nhận",
        2 => "Đã xác nhận",
        3 => "Hoàn thành",
        4 => "Đã hủy",
        5 => "Khách hàng hủy",
        6 => "Không đến",
        _ => "Không xác định"
    };
}
