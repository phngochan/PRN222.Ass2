using PRN222.Ass2.EVDealerSys.BLL.Interfaces;
using PRN222.Ass2.EVDealerSys.BusinessObjects.Models;
using PRN222.Ass2.EVDealerSys.DAL.Interfaces;

namespace PRN222.Ass2.EVDealerSys.BLL.Implementations;
public class PaymentService(IPaymentRepository paymentRepo, IOrderRepository orderRepo) : IPaymentService
{
    private readonly IPaymentRepository _paymentRepo = paymentRepo;
    private readonly IOrderRepository _orderRepo = orderRepo;

    public void ProcessPayment(int orderId, decimal amount)
    {
        var order = _orderRepo.GetById(orderId);
        if (order == null) throw new Exception("Order không tồn tại");

        // 1. Ghi nhận thanh toán
        var payment = new Payment
        {
            OrderId = orderId,
            Amount = amount,
            PaymentMethod = 1, // ví dụ = chuyển khoản
            PaidAt = DateTime.Now,
            Status = "Paid"
        };
        _paymentRepo.Add(payment);

        // 2. Tính tổng đã thanh toán
        var totalPaid = _paymentRepo.GetByOrder(orderId).Sum(p => p.Amount ?? 0);

        // 3. Cập nhật trạng thái đơn hàng
        if (totalPaid >= (order.TotalPrice ?? 0))
        {
            order.Status = 5; // Hoàn tất
        }
        else if (totalPaid > 0)
        {
            order.Status = 3; // Thanh toán một phần
        }

        _orderRepo.Update(order);
    }
}
