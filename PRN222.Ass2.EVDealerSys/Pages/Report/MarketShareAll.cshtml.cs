using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN222.Ass2.EVDealerSys.BLL.Interfaces;
using PRN222.Ass2.EVDealerSys.BusinessObjects.DTO.Report;

namespace PRN222.Ass2.EVDealerSys.Pages.Report;

[Authorize(Roles = "1,2")] // Admin & Manager
public class MarketShareAllModel : PageModel
{
    private readonly IReportService _reportService;
    private readonly IVehicleService _vehicleService;

    public List<MarketShareItemDto> ReportData { get; set; } = new();

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
    public decimal TotalRevenue { get; set; }

    public MarketShareAllModel(IReportService reportService, IVehicleService vehicleService)
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
        ReportData = (await _reportService.GetMarketShareAllAsync(
            FromDate, ToDate, FilterModel, FilterColor, FilterVersion)).ToList();

        // Calculate totals
        TotalQuantity = ReportData.Sum(r => r.Quantity);
        TotalRevenue = ReportData.Sum(r => r.Revenue);
    }
}
