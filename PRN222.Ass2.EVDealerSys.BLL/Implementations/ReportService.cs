using PRN222.Ass2.EVDealerSys.BLL.Interfaces;
using PRN222.Ass2.EVDealerSys.BusinessObjects.DTO.Report;
using PRN222.Ass2.EVDealerSys.DAL.Interfaces;

namespace PRN222.Ass2.EVDealerSys.BLL.Implementations;
public class ReportService(IReportRepository repo) : IReportService
{
    private readonly IReportRepository _repo = repo;

    public Task<IEnumerable<SalesSummaryRowDto>> GetSalesSummaryAsync(DateTime? fromDate, DateTime? toDate)
        => _repo.GetSalesSummaryAsync(fromDate, toDate);

    public Task<IEnumerable<MarketShareItemDto>> GetMarketShareAllAsync(
        DateTime? fromDate, DateTime? toDate, string? model, string? color, string? version)
        => _repo.GetMarketShareAllAsync(fromDate, toDate, model, color, version);

    public Task<IEnumerable<InventoryAllRowDto>> GetInventorySummaryAsync(
        DateTime? fromDate, DateTime? toDate, string? model, string? color, string? version)
        => _repo.GetInventorySummaryAsync(fromDate, toDate, model, color, version);


}
