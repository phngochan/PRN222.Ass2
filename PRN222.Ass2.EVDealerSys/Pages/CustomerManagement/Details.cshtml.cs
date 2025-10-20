using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using PRN222.Ass2.EVDealerSys.Base.BasePageModels;
using PRN222.Ass2.EVDealerSys.BLL.Interfaces;
using PRN222.Ass2.EVDealerSys.Models.CustomerManagement;

namespace PRN222.Ass2.EVDealerSys.Pages.CustomerManagement
{
    [Authorize(Roles = "1,2")]
    public class DetailsModel : BaseCrudPageModel
    {
        private readonly ICustomerService _customerService;
        public DetailsModel(IActivityLogService logService, ICustomerService customerService) : base(logService)
        {
            _customerService = customerService;
        }
        public CustomerDetailsViewModel ViewModel { get; set; } = new();
        public async Task<IActionResult> OnGetAsync(int id)
        {
            try
            {
                var customer = await _customerService.GetCustomerByIdAsync(id);
                if (customer == null)
                {
                    SetError("Không tìm thấy khách hàng.");
                    return RedirectToAction(nameof(Index));
                }

                ViewModel = new CustomerDetailsViewModel
                {
                    Id = customer.Id,
                    Address = customer.Address ?? "",
                    DealerName = customer.Dealer?.Name ?? "",
                    DealerRegion = customer.Dealer?.Region ?? "",
                    Email = customer.Email ?? "",
                    Name = customer.Name ?? "",
                    Phone = customer.Phone ?? "",
                    DealerId = customer.DealerId ?? 0,
                    TotalOrders = customer.Orders?.Count ?? 0,
                    TotalSpent = customer.Orders?.Sum(o => o.TotalPrice ?? 0) ?? 0,
                    RecentOrders = customer.Orders?
                    .OrderByDescending(o => o.OrderDate)
                    .Take(5)
                    .Select(o => new CustomerOrderInfo
                    {
                        OrderId = o.Id,
                        OrderDate = o.OrderDate ?? DateTime.MinValue,
                        TotalAmount = o.TotalPrice ?? 0,
                        Status = o.Status switch
                        {
                            0 => "Chờ xử lý",
                            1 => "Đã xác nhận",
                            2 => "Đang giao hàng",
                            3 => "Đã hoàn thành",
                            4 => "Đã hủy",
                            _ => "Không xác định"
                        },
                        ItemCount = o.OrderItems?.Count ?? 0
                    }).ToList() ?? new List<CustomerOrderInfo>()
                };

                await LogAsync("View Customer Details", $"ID={id}");
                return Page();
            }
            catch (Exception ex)
            {
                SetError("Đã xảy ra lỗi khi tải dữ liệu.");
                await LogAsync("Error", ex.Message);
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
