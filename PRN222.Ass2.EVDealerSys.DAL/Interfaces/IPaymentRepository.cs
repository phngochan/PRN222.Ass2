using PRN222.Ass2.EVDealerSys.BusinessObjects.Models;
using PRN222.Ass2.EVDealerSys.DAL.Base;

namespace PRN222.Ass2.EVDealerSys.DAL.Interfaces
{
    public interface IPaymentRepository : IGenericRepository<Payment>
    {
        void Add(Payment payment);
        IEnumerable<Payment> GetByOrder(int orderId);
    }
}
