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
        await LoadVehiclesAsync();
        LoadStatusOptions();

        if (!ModelState.IsValid)
        {
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

        try
        {
            await _testDriveService.UpdateAsync(dto);
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
