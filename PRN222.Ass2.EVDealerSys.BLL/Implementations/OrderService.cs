using PRN222.Ass2.EVDealerSys.BLL.Interfaces;
using PRN222.Ass2.EVDealerSys.BusinessObjects.Models;
using PRN222.Ass2.EVDealerSys.DAL.Interfaces;

namespace PRN222.Ass2.EVDealerSys.BLL.Implementations;
public class OrderService(
    IOrderRepository orderRepo,
    IOrderItemRepository orderItemRepo,
    IInventoryRepository inventoryRepo,
    IVehicleRepository vehicleRepo) : IOrderService
{
    private readonly IOrderRepository _orderRepo = orderRepo;
    private readonly IOrderItemRepository _orderItemRepo = orderItemRepo;
    private readonly IInventoryRepository _inventoryRepo = inventoryRepo;
    private readonly IVehicleRepository _vehicleRepo = vehicleRepo;

    public void CreateOrder(int customerId, int dealerId, int vehicleId, int quantity)
    {
        var vehicle = _vehicleRepo.GetById(vehicleId);
        if (vehicle == null) throw new Exception("Xe không tồn tại");

        // kiểm tra tồn kho ở EVM
        var stock = _inventoryRepo.GetEvmStock(vehicleId);
        if (stock == null || stock.Quantity < quantity)
        {
            throw new Exception("Không đủ xe trong kho");
        }

        // tạo order
        var order = new Order
        {
            CustomerId = customerId,
            DealerId = dealerId,
            UserId = 1, // demo user
            OrderDate = DateTime.Now,
            Status = 0, // Pending
            TotalPrice = vehicle.Price * quantity
        };

        _orderRepo.Add(order);

        var orderItem = new OrderItem
        {
            OrderId = order.Id,
            VehicleId = vehicleId,
            Quantity = quantity,
            UnitPrice = vehicle.Price
        };

        _orderItemRepo.Create(orderItem);

        // trừ kho ngay khi đặt
        stock.Quantity -= quantity;
        _inventoryRepo.Update(stock);
    }

    public void Update(Order order)
    {
        _orderRepo.Update(order);
    }

    public void CancelOrder(int orderId)
    {
        var order = _orderRepo.GetById(orderId);
        if (order == null) throw new Exception("Không tìm thấy đơn hàng");

        var paid = order.Payments?.Sum(p => p.Amount) ?? 0;
        if (paid > 0)
            throw new Exception("Không thể hủy đơn đã thanh toán");

        foreach (var item in order.OrderItems)
        {
            var inv = _inventoryRepo.GetByVehicle(item.VehicleId ?? 0);
            if (inv != null)
            {
                inv.Quantity += item.Quantity ?? 0;
                _inventoryRepo.Update(inv);
            }
        }

        order.Status = 9; // Cancelled
        _orderRepo.Update(order);
    }
    public void ConfirmOrder(int orderId)
    {
        var order = _orderRepo.GetById(orderId);
        if (order == null) return;

        bool hasStock = true;
        foreach (var item in order.OrderItems)
        {
            var stock = _inventoryRepo.GetEvmStock((int)item.VehicleId);
            if (stock == null || stock.Quantity < item.Quantity)
            {
                hasStock = false;
                break;
            }
        }

        if (hasStock)
        {
            foreach (var item in order.OrderItems)
                _inventoryRepo.ReduceEvmStock((int)item.VehicleId, (int)item.Quantity);

            order.Status = 1; // Confirmed
        }
        else
        {
            order.Status = 2; // WaitingProduction
        }

        _orderRepo.Update(order);
    }

    public void UpdateStatus(int orderId, int status)
    {
        var order = _orderRepo.GetById(orderId);
        if (order == null) return;

        order.Status = status;
        _orderRepo.Update(order);
    }

    public void EditOrder(int orderId, int newVehicleId, int newQuantity)
    {
        var order = _orderRepo.GetById(orderId);
        if (order == null) throw new Exception("Không tìm thấy đơn hàng");

        // Nếu đã có thanh toán thì không cho sửa
        var paid = order.Payments?.Sum(p => p.Amount) ?? 0;
        if (paid > 0)
            throw new Exception("Không thể sửa đơn đã thanh toán");

        var item = order.OrderItems.FirstOrDefault();
        if (item == null) throw new Exception("Đơn hàng không có sản phẩm");

        int oldVehicleId = item.VehicleId ?? 0;
        int oldQty = item.Quantity ?? 0;

        if (oldVehicleId == newVehicleId)
        {
            // cùng loại xe => chỉ điều chỉnh chênh lệch
            var inv = _inventoryRepo.GetByVehicle(newVehicleId);
            if (inv == null) throw new Exception("Không tìm thấy kho");

            int delta = newQuantity - oldQty;
            if (delta > 0 && inv.Quantity < delta)
                throw new Exception("Không đủ xe trong kho");

            inv.Quantity -= delta; // nếu delta âm thì coi như + lại kho
            _inventoryRepo.Update(inv);
        }
        else
        {
            // trả lại stock cũ
            var oldInv = _inventoryRepo.GetByVehicle(oldVehicleId);
            if (oldInv != null)
            {
                oldInv.Quantity += oldQty;
                _inventoryRepo.Update(oldInv);
            }

            // trừ stock mới
            var newInv = _inventoryRepo.GetByVehicle(newVehicleId);
            if (newInv == null || newInv.Quantity < newQuantity)
                throw new Exception("Không đủ xe trong kho cho xe mới");

            newInv.Quantity -= newQuantity;
            _inventoryRepo.Update(newInv);
        }

        // cập nhật item
        var vehicle = _vehicleRepo.GetById(newVehicleId);
        if (vehicle == null) throw new Exception("Xe không tồn tại");

        item.VehicleId = newVehicleId;
        item.Quantity = newQuantity;
        item.UnitPrice = vehicle.Price;

        _orderItemRepo.Update(item);

        // cập nhật tổng giá
        order.TotalPrice = vehicle.Price * newQuantity;
        _orderRepo.Update(order);
    }


    public IEnumerable<Order> GetOrdersByDealer(int dealerId)
        => _orderRepo.GetByDealer(dealerId);

    public async Task<IEnumerable<Order>> GetAllOrdersAsync()
    {
        return await _orderRepo.GetAllAsync();
    }

    public async Task<IEnumerable<Order>> GetOrdersByDealerIdAsync(int dealerId)
    {
        return await _orderRepo.GetByDealerIdAsync(dealerId);
    }

    public async Task<IEnumerable<Order>> GetOrdersByUserIdAsync(int userId)
    {
        return await _orderRepo.GetByUserIdAsync(userId);
    }

    public async Task<int> GetOrdersCountByDateRangeAsync(DateTime startDate, DateTime endDate, int? dealerId = null)
    {
        if (dealerId.HasValue)
        {
            return await _orderRepo.GetCountByDateRangeAndDealerAsync(startDate, endDate, dealerId.Value);
        }
        return await _orderRepo.GetCountByDateRangeAsync(startDate, endDate);
    }

    public async Task<int> GetOrdersCountByUserAndDateRangeAsync(int userId, DateTime startDate, DateTime endDate)
    {
        return await _orderRepo.GetCountByUserAndDateRangeAsync(userId, startDate, endDate);
    }

    public async Task<IEnumerable<Order>> GetRecentOrdersByDealerAsync(int dealerId, int count = 5)
    {
        return await _orderRepo.GetRecentByDealerAsync(dealerId, count);
    }

    // Additional CRUD methods
    public async Task<Order?> GetOrderByIdAsync(int id)
    {
        return await _orderRepo.GetByIdAsync(id);
    }

    public async Task<Order> CreateOrderAsync(Order order)
    {
        return await _orderRepo.CreateAsync(order);
    }

    public async Task<Order> UpdateOrderAsync(Order order)
    {
        return await _orderRepo.UpdateAsync(order);
    }

    public async Task<bool> DeleteOrderAsync(int id)
    {
        return await _orderRepo.DeleteAsync(id);
    }

    public async Task<int> GetTotalOrdersCountAsync()
    {
        return await _orderRepo.GetTotalCountAsync();
    }
}
