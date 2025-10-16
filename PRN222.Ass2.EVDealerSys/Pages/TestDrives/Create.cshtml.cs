using System.Globalization;
using System.Linq;
using System.Security.Claims;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

using PRN222.Ass2.EVDealerSys.BLL.Interfaces;
using PRN222.Ass2.EVDealerSys.BusinessObjects.DTO.TestDrive;
using PRN222.Ass2.EVDealerSys.Models;

namespace PRN222.Ass2.EVDealerSys.Pages.TestDrives;

public class CreateModel : PageModel
{
    private readonly ITestDriveService _testDriveService;
    private readonly IVehicleService _vehicleService;
    private readonly ICustomerService _customerService;
    private readonly ILogger<CreateModel> _logger;

    public CreateModel(
        ITestDriveService testDriveService,
        IVehicleService vehicleService,
        ICustomerService customerService,
        ILogger<CreateModel> logger)
    {
        _testDriveService = testDriveService;
        _vehicleService = vehicleService;
        _customerService = customerService;
        _logger = logger;
    }

    [BindProperty]
    public TestDriveViewModel Form { get; set; } = new();

    public List<SelectListItem> VehicleOptions { get; private set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        await LoadVehiclesAsync();
        EnsureDefaultValues();
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

        if (Form.ScheduledDate.Date <= DateTime.Today)
        {
            ModelState.AddModelError(nameof(Form.ScheduledDate), "Ngày thử xe phải từ ngày mai trở đi.");
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
            Status = Form.Status == 0 ? 1 : Form.Status
        };

        try
        {
            await _testDriveService.CreateAsync(dto);
            TempData["SuccessMessage"] = "Đặt lịch thử xe thành công.";
            return RedirectToPage("./Index");
        }
        catch (ApplicationException ex)
        {
            _logger.LogWarning(ex, "Invalid data while creating test drive booking");
            ModelState.AddModelError(string.Empty, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error when creating test drive booking");
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
            Form.StartTime = new TimeSpan(9, 0, 0);
        }

        if (Form.EndTime == default)
        {
            Form.EndTime = new TimeSpan(10, 0, 0);
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
}
