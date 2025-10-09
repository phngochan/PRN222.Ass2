using PRN222.Ass2.EVDealerSys.BusinessObjects.Models;
using PRN222.Ass2.EVDealerSys.Repositories.Interfaces;
using PRN222.Ass2.EVDealerSys.Repositories.Context;
using PRN222.Ass2.EVDealerSys.Repositories.Base;

namespace PRN222.Ass2.EVDealerSys.Repositories.Implementations
{
    public class OrderItemRepository : GenericRepository<OrderItem>, IOrderItemRepository
    {
        public OrderItemRepository(EvdealerDbContext context) : base(context)
        {
        }

        public IEnumerable<OrderItem> GetByOrder(int orderId)
        {
            return _context.OrderItems.Where(i => i.OrderId == orderId).ToList();
        }
    }
}
