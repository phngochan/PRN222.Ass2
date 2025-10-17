using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;

using PRN222.Ass2.EVDealerSys.BLL.Interfaces;
using PRN222.Ass2.EVDealerSys.BusinessObjects.Models;
using PRN222.Ass2.EVDealerSys.Hubs;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PRN222.Ass2.EVDealerSys.Pages.Orders
{
    public class IndexModel : PageModel
    {
        private readonly IOrderService _orderService;
        private readonly IVehicleService _vehicleService;
        private readonly ICustomerService _customerService;
        private readonly IPaymentService _paymentService;
        private readonly IHubContext<OrderHub> _orderHubContext;

        public List<Order> PagedOrders { get; set; } = new List<Order>();
        public List<Customer> Customers { get; set; } = new List<Customer>();
        public List<VehicleView> Vehicles { get; set; } = new List<VehicleView>();

        [BindProperty] public int CustomerId { get; set; }
        [BindProperty] public int VehicleId { get; set; }
        [BindProperty] public int Quantity { get; set; }
        [BindProperty] public int OrderId { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? SearchCustomer { get; set; }

        [BindProperty(SupportsGet = true)]
        public int PageIndex { get; set; } = 1;

        public int TotalPages { get; set; }

        public IndexModel(
            IOrderService orderService,
            IVehicleService vehicleService,
            ICustomerService customerService,
            IPaymentService paymentService,
            IHubContext<OrderHub> orderHubContext)
        {
            _orderService = orderService;
            _vehicleService = vehicleService;
            _customerService = customerService;
            _paymentService = paymentService;
            _orderHubContext = orderHubContext;
        }

        public void OnGet()
        {
            Customers = _customerService.GetAllCustomers().ToList();
            Vehicles = _vehicleService.GetAllVehicles()
                .Select(v => new VehicleView
                {
                    Id = v.Id,
                    Model = v.Model,
                    Price = v.Price,
                    InventoryQty = _vehicleService.GetInventoryByVehicle(v.Id)?.Quantity ?? 0
                }).ToList();

            var orders = _orderService.GetOrdersByDealer(1).ToList();

            if (!string.IsNullOrEmpty(SearchCustomer))
            {
                orders = orders.Where(o => o.Customer != null &&
                                          o.Customer.Name.Contains(SearchCustomer, StringComparison.OrdinalIgnoreCase))
                                       .ToList();
            }

            int pageSize = 5;
            TotalPages = (int)Math.Ceiling((double)orders.Count / pageSize);
            PagedOrders = orders.Skip((PageIndex - 1) * pageSize).Take(pageSize).ToList();
        }

        public async Task<IActionResult> OnPostCreate()
        {
            try
            {
                var stock = _vehicleService.GetInventoryByVehicle(VehicleId);
                if (stock == null || stock.Quantity < Quantity)
                {
                    TempData["Error"] = "Số lượng vượt quá tồn kho!";
                    return RedirectToPage();
                }

                _orderService.CreateOrder(CustomerId, 1, VehicleId, Quantity);
                TempData["Message"] = "Tạo đơn hàng thành công!";

                var newOrder = _orderService.GetOrdersByDealer(1).OrderByDescending(o => o.Id).FirstOrDefault();
                if (newOrder != null)
                {
                    var orderData = new { /* ... Dữ liệu đơn hàng ... */ };
                    await _orderHubContext.Clients.All.SendAsync("ReceiveNewOrder", orderData);
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostEdit()
        {
            try
            {
                _orderService.EditOrder(OrderId, VehicleId, Quantity); 
                TempData["Message"] = "Cập nhật đơn hàng thành công!";

                var updatedOrder = _orderService.GetOrdersByDealer(1).FirstOrDefault(o => o.Id == OrderId);
                if (updatedOrder != null)
                {
                    var firstItem = updatedOrder.OrderItems.FirstOrDefault();
                    var orderData = new
                    {
                        id = updatedOrder.Id,
                        totalPrice = updatedOrder.TotalPrice?.ToString("N0") + " đ",
                        quantity = firstItem?.Quantity ?? 0,
                        vehicleModel = firstItem?.Vehicle?.Model,
                        unitPrice = firstItem?.UnitPrice?.ToString("N0") + " đ"
                    };

                    await _orderHubContext.Clients.All.SendAsync("ReceiveOrderUpdate", orderData);
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi cập nhật: {ex.Message}";
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostPay()
        {
            try
            {
                var order = _orderService.GetOrdersByDealer(1).FirstOrDefault(o => o.Id == OrderId);
                if (order == null || !order.TotalPrice.HasValue) throw new Exception("Đơn hàng không hợp lệ!");

                _paymentService.ProcessPayment(OrderId, order.TotalPrice.Value);
                TempData["Message"] = "Thanh toán thành công!";

                await _orderHubContext.Clients.All.SendAsync("ReceiveOrderStatusUpdate", new
                {
                    id = order.Id,
                    status = "Đang xử lý"
                });
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostCancel()
        {
            try
            {
                _orderService.CancelOrder(OrderId);
                TempData["Message"] = "Đơn hàng đã được hủy và trả về kho.";

                await _orderHubContext.Clients.All.SendAsync("ReceiveOrderStatusUpdate", new
                {
                    id = OrderId,
                    status = "Đã hủy"
                });
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToPage();
        }

        public class VehicleView
        {
            public int Id { get; set; }
            public string? Model { get; set; }
            public decimal? Price { get; set; }
            public int InventoryQty { get; set; }
        }
    }
}
