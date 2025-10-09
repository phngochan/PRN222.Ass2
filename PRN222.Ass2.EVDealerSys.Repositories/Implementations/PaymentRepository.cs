using PRN222.Ass2.EVDealerSys.BusinessObjects.Models;
using PRN222.Ass2.EVDealerSys.Repositories.Interfaces;
using PRN222.Ass2.EVDealerSys.Repositories.Context;
using PRN222.Ass2.EVDealerSys.Repositories.Base;

namespace PRN222.Ass2.EVDealerSys.Repositories.Implementations
{
    public class PaymentRepository : GenericRepository<Payment>, IPaymentRepository
    {
        public PaymentRepository(EvdealerDbContext context) : base(context)
        {
        }

        public void Add(Payment payment)
        {
            _context.Payments.Add(payment);
            _context.SaveChanges();
        }

        public IEnumerable<Payment> GetByOrder(int orderId)
        {
            return _context.Payments.Where(p => p.OrderId == orderId).ToList();
        }
    }
}

