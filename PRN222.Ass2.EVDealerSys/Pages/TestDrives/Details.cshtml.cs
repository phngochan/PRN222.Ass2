using System;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;

using PRN222.Ass2.EVDealerSys.Base.BasePageModels;
using PRN222.Ass2.EVDealerSys.BLL.Interfaces;
using PRN222.Ass2.EVDealerSys.BusinessObjects.DTO.TestDrive;
using PRN222.Ass2.EVDealerSys.Hubs;
using PRN222.Ass2.EVDealerSys.Models;

namespace PRN222.Ass2.EVDealerSys.Pages.TestDrives;

public class DetailsModel : BaseCrudPageModel
{
    private readonly ITestDriveService _testDriveService;
    private readonly ILogger<DetailsModel> _logger;

    public DetailsModel(
        IActivityLogService logService,
        ITestDriveService testDriveService,
        ILogger<DetailsModel> logger,
        IHubContext<ActivityLogHub> activityLogHubContext) : base(logService)
    {
        _testDriveService = testDriveService;
        _logger = logger;
        SetActivityLogHubContext(activityLogHubContext);
    }

    public TestDriveViewModel TestDrive { get; private set; } = new();

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

            TestDrive = MapToViewModel(dto);
            await LogAsync("View Test Drive Details", $"Viewing Test Drive ID={id}");
            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load test drive details for id {Id}", id);
            await LogAsync("Error", $"Failed to load test drive details: {ex.Message}");
            TempData["ErrorMessage"] = "Không thể tải thông tin lịch thử xe.";
            return RedirectToPage("./Index");
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
