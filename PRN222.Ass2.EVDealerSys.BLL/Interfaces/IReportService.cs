using PRN222.Ass2.EVDealerSys.BusinessObjects.DTO.Report;

namespace PRN222.Ass2.EVDealerSys.BLL.Interfaces;

public interface IReportService
{
    Task<IEnumerable<SalesSummaryRowDto>> GetSalesSummaryAsync(DateTime? fromDate, DateTime? toDate);

    Task<IEnumerable<MarketShareItemDto>> GetMarketShareAllAsync(
        DateTime? fromDate, DateTime? toDate, string? model, string? color, string? version);

    Task<IEnumerable<InventoryAllRowDto>> GetInventorySummaryAsync(
        DateTime? fromDate, DateTime? toDate, string? model, string? color, string? version);

}
