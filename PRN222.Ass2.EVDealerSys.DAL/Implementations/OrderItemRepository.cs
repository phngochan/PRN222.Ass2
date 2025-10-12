using PRN222.Ass2.EVDealerSys.BusinessObjects.Models;
using PRN222.Ass2.EVDealerSys.DAL.Base;
using PRN222.Ass2.EVDealerSys.DAL.Context;
using PRN222.Ass2.EVDealerSys.DAL.Interfaces;

namespace PRN222.Ass2.EVDealerSys.DAL.Implementations
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
