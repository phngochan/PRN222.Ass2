using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

using PRN222.Ass2.EVDealerSys.BLL.Interfaces;
using PRN222.Ass2.EVDealerSys.BusinessObjects.DTO.TestDrive;
using PRN222.Ass2.EVDealerSys.Models;

namespace PRN222.Ass2.EVDealerSys.Pages.TestDrives;

public class EditModel : PageModel
{
    private readonly ITestDriveService _testDriveService;
    private readonly IVehicleService _vehicleService;
    private readonly ILogger<EditModel> _logger;

    public EditModel(
        ITestDriveService testDriveService,
        IVehicleService vehicleService,
        ILogger<EditModel> logger)
    {
        _testDriveService = testDriveService;
        _vehicleService = vehicleService;
        _logger = logger;
    }

    [BindProperty]
    public TestDriveViewModel Form { get; set; } = new();

    public List<SelectListItem> VehicleOptions { get; private set; } = new();
    public List<SelectListItem> StatusOptions { get; private set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        try
        {
            var dto = await _testDriveService.GetByIdAsync(id);
            if (dto == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy lịch thử xe.";
                return RedirectToPage("./Index");
            }

            Form = MapToViewModel(dto);
            await LoadVehiclesAsync();
            LoadStatusOptions();
            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load edit form for test drive {Id}", id);
            TempData["ErrorMessage"] = "Không thể tải thông tin chỉnh sửa.";
            return RedirectToPage("./Index");
        }
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        // Debug logging
        _logger.LogInformation("Edit OnPostAsync called with id={Id}, Form.Id={FormId}", id, Form.Id);
        _logger.LogInformation("Form values: VehicleId={VehicleId}, CustomerId={CustomerId}, ScheduledDate={ScheduledDate}, Status={Status}", 
            Form.VehicleId, Form.CustomerId, Form.ScheduledDate, Form.Status);

        await LoadVehiclesAsync();
        LoadStatusOptions();

        if (!ModelState.IsValid)
        {
            _logger.LogWarning("ModelState is invalid. Errors: {Errors}", 
                string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
            return Page();
        }

        if (Form.Id != id)
        {
            ModelState.AddModelError(string.Empty, "Thông tin lịch thử xe không hợp lệ.");
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

        // Nếu sửa lịch hôm nay, kiểm tra giờ phải sau giờ hiện tại
        if (Form.ScheduledDate.Date == DateTime.Today && Form.StartTime <= DateTime.Now.TimeOfDay)
        {
            ModelState.AddModelError(nameof(Form.StartTime), "Giờ bắt đầu phải sau giờ hiện tại.");
            return Page();
        }

        // Skip time slot availability check for Edit - allow overlapping bookings
        // User requested to not validate vehicle availability when editing

        var dto = new TestDriveDto
        {
            Id = Form.Id,
            VehicleId = Form.VehicleId,
            CustomerId = Form.CustomerId,
            CustomerName = Form.CustomerName ?? string.Empty,
            CustomerPhone = Form.CustomerPhone ?? string.Empty,
            CustomerEmail = Form.CustomerEmail ?? string.Empty,
            ScheduledDate = Form.ScheduledDate,
            StartTime = Form.StartTime,
            EndTime = Form.EndTime,
            Notes = string.IsNullOrWhiteSpace(Form.Notes) ? null : Form.Notes.Trim(),
            DealerId = Form.DealerId,
            UserId = Form.UserId,
            Status = Form.Status
        };

        _logger.LogInformation("DTO created: Id={Id}, VehicleId={VehicleId}, CustomerId={CustomerId}, StartTime={StartTime}, EndTime={EndTime}, Status={Status}", 
            dto.Id, dto.VehicleId, dto.CustomerId, dto.StartTime, dto.EndTime, dto.Status);

        try
        {
            var result = await _testDriveService.UpdateAsync(dto);
            _logger.LogInformation("UpdateAsync completed. Result: {ResultId}", result?.Id);
            TempData["SuccessMessage"] = "Cập nhật lịch thử xe thành công.";
            return RedirectToPage("./Index");
        }
        catch (ApplicationException ex)
        {
            _logger.LogWarning(ex, "Validation failure when updating test drive {Id}", id);
            ModelState.AddModelError(string.Empty, ex.Message);
            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error when updating test drive {Id}", id);
            ModelState.AddModelError(string.Empty, "Có lỗi xảy ra khi cập nhật lịch thử xe.");
            return Page();
        }
    }

    public string GetStatusBadgeClass(int status) => status switch
    {
        1 => "warning",
        2 => "primary",
        3 => "success",
        4 => "danger",
        5 => "secondary",
        6 => "dark",
        _ => "secondary"
    };

    public string GetStatusName(int status) => status switch
    {
        1 => "Chờ xác nhận",
        2 => "Đã xác nhận",
        3 => "Hoàn thành",
        4 => "Đã hủy",
        5 => "Khách hàng hủy",
        6 => "Không đến",
        _ => "Không xác định"
    };

    private async Task LoadVehiclesAsync()
    {
        var vehicles = await _vehicleService.GetAllVehiclesActiveAsync();
        VehicleOptions = vehicles
            .OrderBy(v => v.Model)
            .Select(v => new SelectListItem
            {
                Value = v.Id.ToString(),
                Text = string.IsNullOrWhiteSpace(v.Version) ? v.Model ?? "Xe" : $"{v.Model} - {v.Version}",
                Selected = v.Id == Form.VehicleId
            })
            .ToList();
    }

    private void LoadStatusOptions()
    {
        var statuses = new Dictionary<int, string>
        {
            { 1, "Chờ xác nhận" },
            { 2, "Đã xác nhận" },
            { 3, "Hoàn thành" },
            { 4, "Đã huỷ" },
            { 5, "Khách hàng huỷ" },
            { 6, "Không đến" }
        };

        StatusOptions = statuses
            .Select(kvp => new SelectListItem
            {
                Value = kvp.Key.ToString(),
                Text = kvp.Value,
                Selected = kvp.Key == Form.Status
            })
            .ToList();
    }

    private static TestDriveViewModel MapToViewModel(TestDriveDto dto)
    {
        return new TestDriveViewModel
        {
            Id = dto.Id,
            VehicleId = dto.VehicleId,
            CustomerId = dto.CustomerId,
            CustomerName = dto.CustomerName,
            CustomerPhone = dto.CustomerPhone,
            CustomerEmail = dto.CustomerEmail,
            ScheduledDate = dto.ScheduledDate,
            StartTime = dto.StartTime,
            EndTime = dto.EndTime,
            Notes = dto.Notes,
            DealerId = dto.DealerId,
            UserId = dto.UserId,
            Status = dto.Status,
            VehicleName = dto.VehicleName,
            VehicleModel = dto.VehicleModel,
            DealerName = dto.DealerName,
            UserName = dto.UserName,
            StatusName = dto.StatusName,
            CreatedAt = dto.CreatedAt
        };
    }
}
