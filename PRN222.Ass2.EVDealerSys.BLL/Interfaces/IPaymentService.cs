namespace PRN222.Ass2.EVDealerSys.BLL.Interfaces;

public interface IPaymentService
{
    void ProcessPayment(int orderId, decimal amount);
}