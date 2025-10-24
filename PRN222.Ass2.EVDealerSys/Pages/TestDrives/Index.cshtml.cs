
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using PRN222.Ass2.EVDealerSys.Base.BasePageModels;
using PRN222.Ass2.EVDealerSys.BLL.Interfaces;
using PRN222.Ass2.EVDealerSys.BusinessObjects.DTO.TestDrive;
using PRN222.Ass2.EVDealerSys.Hubs;
using PRN222.Ass2.EVDealerSys.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
namespace PRN222.Ass2.EVDealerSys.Pages.TestDrives;

[Authorize(Roles = "1,2,3")] // Admin, Manager, Staff có thể xem lịch thử xe
public class IndexModel : BaseCrudPageModel
{
    private readonly ITestDriveService _testDriveService;
    private readonly IVehicleService _vehicleService;
    private readonly IHubContext<TestDriveHub> _testDriveHubContext;
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(
        IActivityLogService logService,
        ITestDriveService testDriveService,
        IVehicleService vehicleService,
        ILogger<IndexModel> logger,
        IHubContext<TestDriveHub> testDriveHubContext,
        IHubContext<ActivityLogHub> activityLogHubContext) : base(logService)
    {
        _testDriveService = testDriveService;
        _vehicleService = vehicleService;
        _testDriveHubContext = testDriveHubContext;
        _logger = logger;
        SetActivityLogHubContext(activityLogHubContext);
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
        await LogAsync("View Test Drives List", $"SearchTerm={SearchTerm}, FilterStatus={FilterStatus}, FilterVehicle={FilterVehicle}");
    }

    public async Task<IActionResult> OnPostUpdateStatusAsync(int id, int status)
    {
        try
        {
            // Get test drive info
            var testDrive = await _testDriveService.GetByIdAsync(id);
            if (testDrive == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy lịch thử xe.";
                return RedirectToPage(new { SearchTerm, FilterStatus, FilterDate = FilterDate?.ToString("yyyy-MM-dd"), FilterVehicle });
            }
            
            var oldStatus = testDrive.Status;
            
            // **VALIDATION: Chỉ cho phép hoàn thành (status=3) hoặc không đến (status=6) sau khi qua giờ kết thúc**
            if ((status == 3 || status == 6) && oldStatus == 2)
            {
                var testDriveEndDateTime = testDrive.ScheduledDate.Add(testDrive.EndTime);
                if (DateTime.Now <= testDriveEndDateTime)
                {
                    TempData["ErrorMessage"] = status == 3 
                        ? $"Chỉ có thể đánh dấu hoàn thành sau khi qua giờ kết thúc ({testDriveEndDateTime:dd/MM/yyyy HH:mm})."
                        : $"Chỉ có thể đánh dấu 'Không đến' sau khi qua giờ kết thúc ({testDriveEndDateTime:dd/MM/yyyy HH:mm}).";
                    
                    return RedirectToPage(new { SearchTerm, FilterStatus, FilterDate = FilterDate?.ToString("yyyy-MM-dd"), FilterVehicle });
                }
            }
            
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
                    _ => "Unknown"
                };
                
                await LogAsync("Update Test Drive Status", $"Updated Test Drive ID={id} to Status={statusName}");
                
                // Notify via SignalR
                await _testDriveHubContext.Clients.All.SendAsync("TestDriveStatusUpdated", new
                {
                    testDriveId = id,
                    newStatus = status,
                    statusName = statusName
                });
                
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
            await LogAsync("Error", $"Failed to update test drive status: {ex.Message}");
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

    // **METHOD MỚI: Cancel với lý do**
    public async Task<IActionResult> OnPostCancelWithReasonAsync(int id, int status, string? cancelReason)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(cancelReason))
            {
                TempData["ErrorMessage"] = "Vui lòng nhập lý do hủy lịch.";
                return RedirectToPage(new { SearchTerm, FilterStatus, FilterDate = FilterDate?.ToString("yyyy-MM-dd"), FilterVehicle });
            }

            var testDrive = await _testDriveService.GetByIdAsync(id);
            if (testDrive == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy lịch thử xe.";
                return RedirectToPage(new { SearchTerm, FilterStatus, FilterDate = FilterDate?.ToString("yyyy-MM-dd"), FilterVehicle });
            }

            // Cập nhật status và thêm lý do hủy vào Notes
            var currentNotes = string.IsNullOrEmpty(testDrive.Notes) ? "" : testDrive.Notes + "\n\n";
            var cancelNote = $"[Lý do hủy - {DateTime.Now:dd/MM/yyyy HH:mm}]: {cancelReason.Trim()}";
            testDrive.Notes = currentNotes + cancelNote;
            testDrive.Status = status;

            var updated = await _testDriveService.UpdateAsync(testDrive);
            
            if (updated != null)
            {
                var statusName = status == 4 ? "Đã hủy" : "Khách hàng hủy";
                
                await LogAsync("Cancel Test Drive", $"Cancelled Test Drive ID={id}, Status={statusName}, Reason: {cancelReason}");
                
                // Notify via SignalR
                await _testDriveHubContext.Clients.All.SendAsync("TestDriveStatusUpdated", new
                {
                    testDriveId = id,
                    newStatus = status,
                    statusName = statusName
                });
                
                TempData["SuccessMessage"] = $"{statusName} thành công. Lý do: {cancelReason}";
            }
            else
            {
                TempData["ErrorMessage"] = "Không thể hủy lịch thử xe.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to cancel test drive with reason for id {Id}", id);
            await LogAsync("Error", $"Failed to cancel test drive: {ex.Message}");
            TempData["ErrorMessage"] = "Có lỗi xảy ra khi hủy lịch thử xe.";
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
