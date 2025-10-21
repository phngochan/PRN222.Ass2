using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN222.Ass2.EVDealerSys.BLL.Interfaces;
using PRN222.Ass2.EVDealerSys.BusinessObjects.DTO.Report;

namespace PRN222.Ass2.EVDealerSys.Pages.Report;

[Authorize(Roles = "1,2,3")] // Admin, Manager & Staff
public class InventoryAllModel : PageModel
{
    private readonly IReportService _reportService;
    private readonly IVehicleService _vehicleService;

    public List<InventoryAllRowDto> ReportData { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public DateTime? FromDate { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateTime? ToDate { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? FilterModel { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? FilterColor { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? FilterVersion { get; set; }

    public List<string> AvailableModels { get; set; } = new();
    public List<string> AvailableColors { get; set; } = new();

    public int TotalQuantity { get; set; }
    public int TotalSold { get; set; }

    public InventoryAllModel(IReportService reportService, IVehicleService vehicleService)
    {
        _reportService = reportService;
        _vehicleService = vehicleService;
    }

    public async Task OnGetAsync()
    {
        // Load filter options
        AvailableModels = await _vehicleService.GetDistinctModelsAsync();
        AvailableColors = await _vehicleService.GetDistinctColorsAsync();

        // Load report data
        ReportData = (await _reportService.GetInventorySummaryAsync(
            FromDate, ToDate, FilterModel, FilterColor, FilterVersion)).ToList();

        // Calculate totals
        TotalQuantity = ReportData.Sum(r => r.Quantity);
        TotalSold = ReportData.Sum(r => r.Sold);
    }
}
