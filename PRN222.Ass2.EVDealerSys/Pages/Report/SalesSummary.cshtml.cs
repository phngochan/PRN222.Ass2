using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN222.Ass2.EVDealerSys.BLL.Interfaces;
using PRN222.Ass2.EVDealerSys.BusinessObjects.DTO.Report;

namespace PRN222.Ass2.EVDealerSys.Pages.Report;

[Authorize(Roles = "1,2")] // Admin & Manager
public class SalesSummaryModel : PageModel
{
    private readonly IReportService _reportService;

    public List<SalesSummaryRowDto> ReportData { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public DateTime? FromDate { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateTime? ToDate { get; set; }

    public decimal GrandTotalRevenue { get; set; }
    public decimal GrandTotalPaid { get; set; }
    public decimal GrandTotalDebt { get; set; }
    public int GrandTotalOrders { get; set; }
    public int GrandTotalVehicles { get; set; }

    public SalesSummaryModel(IReportService reportService)
    {
        _reportService = reportService;
    }

    public async Task OnGetAsync()
    {
        ReportData = (await _reportService.GetSalesSummaryAsync(FromDate, ToDate)).ToList();

        // Calculate grand totals
        GrandTotalRevenue = ReportData.Sum(r => r.TotalRevenue);
        GrandTotalPaid = ReportData.Sum(r => r.TotalPaid);
        GrandTotalDebt = ReportData.Sum(r => r.TotalDebt);
        GrandTotalOrders = ReportData.Sum(r => r.TotalOrders);
        GrandTotalVehicles = ReportData.Sum(r => r.TotalVehicles);
    }
}
