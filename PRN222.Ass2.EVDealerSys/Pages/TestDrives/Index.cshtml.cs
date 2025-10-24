using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.SignalR;

using PRN222.Ass2.EVDealerSys.BLL.Interfaces;
using PRN222.Ass2.EVDealerSys.BusinessObjects.DTO.TestDrive;
using PRN222.Ass2.EVDealerSys.Hubs;
using PRN222.Ass2.EVDealerSys.Models;

namespace PRN222.Ass2.EVDealerSys.Pages.TestDrives;

public class IndexModel : PageModel
{
    private readonly ITestDriveService _testDriveService;
    private readonly IVehicleService _vehicleService;
    private readonly IHubContext<TestDriveHub> _hubContext;
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(
        ITestDriveService testDriveService,
        IVehicleService vehicleService,
        IHubContext<TestDriveHub> hubContext,
        ILogger<IndexModel> logger)
    {
        _testDriveService = testDriveService;
        _vehicleService = vehicleService;
        _hubContext = hubContext;
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
            // Get old status before update for notification
            var testDrive = await _testDriveService.GetByIdAsync(id);
            var oldStatus = testDrive?.Status ?? 0;
            
            var updated = await _testDriveService.UpdateStatusAsync(id, status);
            
            if (updated)
            {
                var statusName = status switch
                {
                    1 => "Chờ xác nhận",
                    2 => "Đã xác nhận",
                    3 => "Hoàn thành",
                    4 => "Đã hủy",
                    5 => "Khách hàng hủy",
                    6 => "Không đến",
                    _ => "Không xác định"
                };
                
                // Send real-time notification via SignalR
                await _hubContext.Clients.All.SendAsync("TestDriveStatusChanged", new
                {
                    testDriveId = id,
                    oldStatus,
                    newStatus = status,
                    statusName,
                    customerName = testDrive?.CustomerName,
                    vehicleName = testDrive?.VehicleName,
                    timestamp = DateTime.Now
                });
                
                _logger.LogInformation("Test drive status changed and SignalR notification sent: {Id} from {OldStatus} to {NewStatus}", 
                    id, oldStatus, status);
                
                TempData["SuccessMessage"] = status switch
                {
                    1 => "Cập nhật trạng thái thành chờ xác nhận.",
                    2 => "Xác nhận lịch thử xe thành công.",
                    3 => "Đánh dấu lịch thử xe đã hoàn thành.",
                    4 => "Hủy lịch thử xe thành công.",
                    5 => "Đánh dấu khách hàng hủy lịch.",
                    6 => "Ghi nhận khách hàng không đến.",
                    _ => "Cập nhật trạng thái thành công."
                };
            }
            else
            {
                TempData["ErrorMessage"] = status == 6 
                    ? "Không thể đánh dấu 'Không đến'. Chỉ có thể đánh dấu sau khi qua giờ thử xe và khi trạng thái là 'Đã xác nhận'."
                    : "Không thể cập nhật trạng thái. Kiểm tra xem trạng thái chuyển đổi có hợp lệ không.";
            }
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
