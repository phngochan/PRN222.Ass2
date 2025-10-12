using PRN222.Ass2.EVDealerSys.BusinessObjects.Models;

namespace PRN222.Ass2.EVDealerSys.BLL.Interfaces;
public interface IOrderService
{
    void CreateOrder(int customerId, int dealerId, int vehicleId, int quantity);
    void Update(Order order);
    void CancelOrder(int orderId);
    void ConfirmOrder(int orderId);
    void UpdateStatus(int orderId, int status);
    void EditOrder(int orderId, int newVehicleId, int newQuantity);
    IEnumerable<Order> GetOrdersByDealer(int dealerId);
    //
    Task<IEnumerable<Order>> GetAllOrdersAsync();
    Task<IEnumerable<Order>> GetOrdersByDealerIdAsync(int dealerId);
    Task<IEnumerable<Order>> GetOrdersByUserIdAsync(int userId);
    Task<int> GetOrdersCountByDateRangeAsync(DateTime startDate, DateTime endDate, int? dealerId = null);
    Task<int> GetOrdersCountByUserAndDateRangeAsync(int userId, DateTime startDate, DateTime endDate);
    Task<IEnumerable<Order>> GetRecentOrdersByDealerAsync(int dealerId, int count = 5);
}
