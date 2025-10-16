using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using PRN222.Ass2.EVDealerSys.BLL.Interfaces;
using PRN222.Ass2.EVDealerSys.BusinessObjects.DTO.TestDrive;
using PRN222.Ass2.EVDealerSys.Models;

namespace PRN222.Ass2.EVDealerSys.Pages.TestDrives;

public class DashboardModel : PageModel
{
    private readonly ITestDriveService _testDriveService;
    private readonly ILogger<DashboardModel> _logger;

    public DashboardModel(ITestDriveService testDriveService, ILogger<DashboardModel> logger)
    {
        _testDriveService = testDriveService;
        _logger = logger;
    }

    public TestDriveDashboardViewModel Dashboard { get; private set; } = new();

    public async Task OnGetAsync()
    {
        try
        {
            var dealerId = GetDealerId();
            var dto = await _testDriveService.GetDashboardDataAsync(dealerId);
            Dashboard = MapDashboard(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load test drive dashboard data");
            TempData["ErrorMessage"] = "Không thể tải dữ liệu dashboard thử xe.";
            Dashboard = new TestDriveDashboardViewModel();
        }
    }

    public async Task<IActionResult> OnPostUpdateStatusAsync(int id, int status)
    {
        try
        {
            var updated = await _testDriveService.UpdateStatusAsync(id, status);
            
            if (updated)
            {
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
                TempData["ErrorMessage"] = "Không thể cập nhật trạng thái. Kiểm tra xem trạng thái chuyển đổi có hợp lệ không.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update test drive status for id {Id}", id);
            TempData["ErrorMessage"] = "Có lỗi xảy ra khi cập nhật trạng thái.";
        }

        return RedirectToPage();
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

    private static TestDriveDashboardViewModel MapDashboard(TestDriveDashboardDto dto)
    {
        return new TestDriveDashboardViewModel
        {
            TodayTestDrivesCount = dto.TodayTestDrivesCount,
            PendingTestDrives = dto.PendingTestDrives,
            CompletedTestDrives = dto.CompletedTestDrives,
            TotalTestDrives = dto.TotalTestDrives,
            UpcomingTestDrives = dto.UpcomingTestDrives.Select(MapItem).ToList(),
            TodayTestDrives = dto.TodayTestDrives.Select(MapItem).ToList()
        };
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
