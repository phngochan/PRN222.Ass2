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

public class IndexModel : PageModel
{
    private readonly ITestDriveService _testDriveService;
    private readonly IVehicleService _vehicleService;
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(
        ITestDriveService testDriveService,
        IVehicleService vehicleService,
        ILogger<IndexModel> logger)
    {
        _testDriveService = testDriveService;
        _vehicleService = vehicleService;
        _logger = logger;
    }

    [BindProperty(SupportsGet = true)]
    public string? SearchTerm { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? FilterStatus { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateTime? FilterDate { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? FilterVehicle { get; set; }

    public TestDriveListViewModel ViewModel { get; private set; } = new();

    public List<SelectListItem> VehicleOptions { get; private set; } = new();

    public async Task OnGetAsync()
    {
        await LoadVehiclesAsync();
        await LoadTestDrivesAsync();
    }

    public async Task<IActionResult> OnPostUpdateStatusAsync(int id, int status)
    {
        try
        {
            var updated = await _testDriveService.UpdateStatusAsync(id, status);
            TempData[updated ? "SuccessMessage" : "ErrorMessage"] = updated
                ? "Cập nhật trạng thái thành công."
                : "Không thể cập nhật trạng thái.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update test drive status for id {Id}", id);
            TempData["ErrorMessage"] = "Có lỗi xảy ra khi cập nhật trạng thái.";
        }

        return RedirectToPage(new
        {
            SearchTerm,
            FilterStatus,
            FilterDate = FilterDate?.ToString("yyyy-MM-dd"),
            FilterVehicle
        });
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

    private async Task LoadVehiclesAsync()
    {
        var vehicles = await _vehicleService.GetAllVehiclesActiveAsync();

        VehicleOptions = vehicles
            .OrderBy(v => v.Model)
            .Select(v => new SelectListItem
            {
                Value = v.Id.ToString(),
                Text = string.IsNullOrWhiteSpace(v.Version) ? v.Model ?? "Xe" : $"{v.Model} - {v.Version}"
            })
            .ToList();
    }

    private async Task LoadTestDrivesAsync()
    {
        try
        {
            var dealerId = GetDealerId();
            var dtos = await _testDriveService.SearchAsync(SearchTerm, FilterStatus, FilterDate, FilterVehicle, dealerId);

            ViewModel = new TestDriveListViewModel
            {
                SearchTerm = SearchTerm,
                FilterStatus = FilterStatus,
                FilterDate = FilterDate,
                FilterVehicle = FilterVehicle,
                TestDrives = dtos.Select(MapItem).ToList()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load test drive list with filters {@Filters}", new
            {
                SearchTerm,
                FilterStatus,
                FilterDate,
                FilterVehicle
            });

            TempData["ErrorMessage"] = "Không thể tải danh sách lịch thử xe.";
            ViewModel = new TestDriveListViewModel();
        }
    }

    private static TestDriveItemViewModel MapItem(TestDriveItemDto dto)
    {
        return new TestDriveItemViewModel
        {
            Id = dto.Id,
            CustomerName = dto.CustomerName,
            CustomerPhone = dto.CustomerPhone,
            CustomerEmail = dto.CustomerEmail,
            VehicleName = dto.VehicleName ?? string.Empty,
            VehicleModel = dto.VehicleModel ?? string.Empty,
            ScheduledDate = dto.ScheduledDate,
            StartTime = dto.StartTime,
            EndTime = dto.EndTime,
            Status = dto.Status,
            StatusName = dto.StatusName,
            Notes = dto.Notes,
            CreatedAt = dto.CreatedAt
        };
    }

    private int? GetDealerId()
    {
        var dealerClaim = User.FindFirst("DealerId")?.Value;
        return int.TryParse(dealerClaim, out var dealerId) ? dealerId : null;
    }
}
