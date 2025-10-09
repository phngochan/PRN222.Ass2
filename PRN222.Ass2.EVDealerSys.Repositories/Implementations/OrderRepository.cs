using PRN222.Ass2.EVDealerSys.BusinessObjects.Models;
using PRN222.Ass2.EVDealerSys.Repositories.Interfaces;
using PRN222.Ass2.EVDealerSys.Repositories.Context;
using Microsoft.EntityFrameworkCore;
using PRN222.Ass2.EVDealerSys.Repositories.Base;

namespace PRN222.Ass2.EVDealerSys.Repositories.Implementations
{
    public class OrderRepository : GenericRepository<Order>, IOrderRepository
    {
        public OrderRepository(EvdealerDbContext context) : base(context)
        {
        }

        public void Add(Order order)
        {
            _context.Orders.Add(order);
            _context.SaveChanges();
        }

        public override Order? GetById(int id)
        {
            return _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Vehicle)
                .Include(o => o.Payments)
                .FirstOrDefault(o => o.Id == id);
        }

        public IEnumerable<Order> GetByDealer(int dealerId)
        {
            return _context.Orders
                .Include(o => o.Dealer)      
                .Include(o => o.Customer)   
                .Include(o => o.OrderItems).ThenInclude(oi => oi.Vehicle)
                .Include(o => o.Payments)
                .Where(o => o.DealerId == dealerId)
                .ToList();
        }

        public override async Task<IEnumerable<Order>> GetAllAsync()
        {
            return await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Vehicle)
                .Include(o => o.User)
                    .ThenInclude(u => u.Dealer)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        public override async Task<Order?> GetByIdAsync(int id)
        {
            return await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Vehicle)
                .Include(o => o.User)
                    .ThenInclude(u => u.Dealer)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        // Methods for OrderService dashboard functions
        public async Task<IEnumerable<Order>> GetByDealerIdAsync(int dealerId)
        {
            return await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Vehicle)
                .Include(o => o.User)
                    .ThenInclude(u => u.Dealer)
                .Where(o => o.User != null && o.User.DealerId == dealerId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Order>> GetByUserIdAsync(int userId)
        {
            return await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Vehicle)
                .Include(o => o.User)
                    .ThenInclude(u => u.Dealer)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Order>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Vehicle)
                .Include(o => o.User)
                    .ThenInclude(u => u.Dealer)
                .Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Order>> GetByDateRangeAndDealerAsync(DateTime startDate, DateTime endDate, int dealerId)
        {
            return await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Vehicle)
                .Include(o => o.User)
                    .ThenInclude(u => u.Dealer)
                .Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate
                           && o.User != null && o.User.DealerId == dealerId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Order>> GetByUserAndDateRangeAsync(int userId, DateTime startDate, DateTime endDate)
        {
            return await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Vehicle)
                .Include(o => o.User)
                    .ThenInclude(u => u.Dealer)
                .Where(o => o.UserId == userId && o.OrderDate >= startDate && o.OrderDate <= endDate)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Order>> GetRecentByDealerAsync(int dealerId, int count = 5)
        {
            return await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Vehicle)
                .Include(o => o.User)
                    .ThenInclude(u => u.Dealer)
                .Where(o => o.User != null && o.User.DealerId == dealerId)
                .OrderByDescending(o => o.OrderDate)
                .Take(count)
                .ToListAsync();
        }

        // Count methods for dashboard statistics
        public async Task<int> GetCountByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Orders
                .Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate)
                .CountAsync();
        }

        public async Task<int> GetCountByDateRangeAndDealerAsync(DateTime startDate, DateTime endDate, int dealerId)
        {
            return await _context.Orders
                .Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate
                           && o.User != null && o.User.DealerId == dealerId)
                .CountAsync();
        }

        public async Task<int> GetCountByUserAndDateRangeAsync(int userId, DateTime startDate, DateTime endDate)
        {
            return await _context.Orders
                .Where(o => o.UserId == userId && o.OrderDate >= startDate && o.OrderDate <= endDate)
                .CountAsync();
        }

        public async Task<int> GetTotalCountAsync()
        {
            return await _context.Orders.CountAsync();
        }
    }
}
