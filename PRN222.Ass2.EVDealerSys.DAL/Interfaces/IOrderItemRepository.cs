using PRN222.Ass2.EVDealerSys.BusinessObjects.Models;
using PRN222.Ass2.EVDealerSys.DAL.Base;

namespace PRN222.Ass2.EVDealerSys.DAL.Interfaces
{
    public interface IOrderItemRepository : IGenericRepository<OrderItem>
    {
        IEnumerable<OrderItem> GetByOrder(int orderId);
    }
}
