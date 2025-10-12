using PRN222.Ass2.EVDealerSys.BusinessObjects.Models;
using PRN222.Ass2.EVDealerSys.DAL.Base;

namespace PRN222.Ass2.EVDealerSys.DAL.Interfaces
{
    public interface IOrderRepository : IGenericRepository<Order>
    {
        // CRUD Operations
        void Add(Order order);

        // Query Methods
        IEnumerable<Order> GetByDealer(int dealerId);

        Task<IEnumerable<Order>> GetByDealerIdAsync(int dealerId);
        Task<IEnumerable<Order>> GetByUserIdAsync(int userId);
        Task<IEnumerable<Order>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<Order>> GetByDateRangeAndDealerAsync(DateTime startDate, DateTime endDate, int dealerId);
        Task<IEnumerable<Order>> GetByUserAndDateRangeAsync(int userId, DateTime startDate, DateTime endDate);
        Task<IEnumerable<Order>> GetRecentByDealerAsync(int dealerId, int count = 5);

        // Count methods for dashboard statistics
        Task<int> GetCountByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<int> GetCountByDateRangeAndDealerAsync(DateTime startDate, DateTime endDate, int dealerId);
        Task<int> GetCountByUserAndDateRangeAsync(int userId, DateTime startDate, DateTime endDate);
        Task<int> GetTotalCountAsync();
    }
}
