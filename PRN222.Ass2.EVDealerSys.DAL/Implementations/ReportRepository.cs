using PRN222.Ass2.EVDealerSys.DAL.Interfaces;
using PRN222.Ass2.EVDealerSys.DAL.Context;
using PRN222.Ass2.EVDealerSys.BusinessObjects.DTO.Report;
using Microsoft.EntityFrameworkCore;

namespace PRN222.Ass2.EVDealerSys.DAL.Implementations
{
    public class ReportRepository : IReportRepository
    {
        private readonly EvdealerDbContext _context;

        public ReportRepository(EvdealerDbContext context) => _context = context;

        // Sales Summary Method
        public async Task<IEnumerable<SalesSummaryRowDto>> GetSalesSummaryAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = _context.Orders
                .Include(o => o.OrderItems)
                .Include(o => o.Payments)
                .Include(o => o.Dealer)
                .AsQueryable();

            if (fromDate.HasValue) query = query.Where(o => o.OrderDate >= fromDate.Value);
            if (toDate.HasValue) query = query.Where(o => o.OrderDate <= toDate.Value);

            var rows = await query
                .GroupBy(o => o.Dealer != null ? o.Dealer.Name : "(Unknown Dealer)")
                .Select(g => new SalesSummaryRowDto
                {
                    Dealer = g.Key!,
                    TotalOrders = g.Count(),
                    TotalVehicles = (int)g.SelectMany(o => o.OrderItems).Sum(oi => oi.Quantity),
                    TotalRevenue = (decimal)g.Sum(o => o.TotalPrice),
                    TotalPaid = g.SelectMany(o => o.Payments)
                                     .Sum(p => (decimal?)p.Amount) ?? 0,
                    TotalDebt = (decimal)(g.Sum(o => o.TotalPrice)
                                   - (g.SelectMany(o => o.Payments)
                                       .Sum(p => (decimal?)p.Amount) ?? 0))
                })
                .ToListAsync();

            return rows;
        }

        // Market Share Method
        public async Task<IEnumerable<MarketShareItemDto>> GetMarketShareAllAsync(DateTime? fromDate, DateTime? toDate, string? model, string? color, string? version)
        {
            var q = _context.Orders
                .Include(o => o.OrderItems).ThenInclude(oi => oi.Vehicle)
                .Include(o => o.Dealer)
                .Where(o => (!fromDate.HasValue || o.OrderDate >= fromDate)
                         && (!toDate.HasValue || o.OrderDate <= toDate))
                .SelectMany(o => o.OrderItems.Select(oi => new
                {
                    o.Dealer.Name,
                    oi.Vehicle.Model,
                    oi.Vehicle.Color,
                    oi.Vehicle.Version,
                    Qty = oi.Quantity,
                    Rev = oi.UnitPrice * oi.Quantity
                }));

            if (!string.IsNullOrEmpty(model)) q = q.Where(x => x.Model == model);
            if (!string.IsNullOrEmpty(color)) q = q.Where(x => x.Color == color);
            if (!string.IsNullOrEmpty(version)) q = q.Where(x => x.Version == version);

            return await q.GroupBy(x => new { x.Model, x.Color, x.Version })
                .Select(g => new MarketShareItemDto
                {
                    Model = g.Key.Model,
                    Color = g.Key.Color,
                    Version = g.Key.Version,
                    Quantity = (int)g.Sum(x => x.Qty),
                    Revenue = (decimal)g.Sum(x => x.Rev)
                })
                .AsNoTracking().ToListAsync();
        }

        public async Task<IEnumerable<InventoryAllRowDto>> GetInventorySummaryAsync(
    DateTime? fromDate, DateTime? toDate, string? model, string? color, string? version)
        {
            // Default values for DateTime if null
            DateTime validFromDate = fromDate ?? new DateTime(1753, 1, 1);  // SQL Server DateTime minimum
            DateTime validToDate = toDate ?? new DateTime(9999, 12, 31);    // SQL Server DateTime maximum

            // 1) Tồn kho hiện tại theo Dealer x Vehicle
            var inv = _context.Inventories
                .Include(i => i.Dealer)
                .Include(i => i.Vehicle)
                .Select(i => new
                {
                    i.DealerId,
                    DealerName = i.Dealer.Name,
                    i.VehicleId,
                    i.Vehicle.Model,
                    i.Vehicle.Color,
                    i.Vehicle.Version,
                    QtyOnHand = (int?)i.Quantity   // đảm bảo kiểu nullable
                });

            // 2) Số lượng bán trong khoảng ngày theo Dealer x Vehicle (lọc ngày ở Orders)
            var soldInRange = _context.Orders
                .Include(o => o.OrderItems)
                .Where(o => (!fromDate.HasValue || o.OrderDate >= fromDate.Value) &&
                            (!toDate.HasValue || o.OrderDate <= toDate.Value))
                .SelectMany(o => o.OrderItems.Select(oi => new
                {
                    o.DealerId,
                    oi.VehicleId,
                    Qty = (int?)oi.Quantity  // cast sang nullable
                }))
                .GroupBy(x => new { x.DealerId, x.VehicleId })
                .Select(g => new
                {
                    g.Key.DealerId,
                    g.Key.VehicleId,
                    Sold = g.Sum(x => x.Qty) // Sum của int? => ra int?
                });

            // 3) Join tồn kho với số bán, rồi áp dụng filter Model/Color/Version
            var query =
                from i in inv
                join s in soldInRange
                    on new { i.DealerId, i.VehicleId } equals new { s.DealerId, s.VehicleId } into gj
                from s in gj.DefaultIfEmpty()
                where (string.IsNullOrEmpty(model) || i.Model == model)
                   && (string.IsNullOrEmpty(color) || i.Color == color)
                   && (string.IsNullOrEmpty(version) || i.Version == version)
                select new InventoryAllRowDto
                {
                    Dealer = i.DealerName,
                    Model = i.Model,
                    Color = i.Color,
                    Version = i.Version,
                    Quantity = i.QtyOnHand ?? 0,              // Fix null → 0
                    Sold = s != null ? (s.Sold ?? 0) : 0      // Fix null → 0
                };

            return await query.AsNoTracking().ToListAsync();
        }



    }
}
