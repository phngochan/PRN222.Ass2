using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

using PRN222.Ass2.EVDealerSys.Base.BasePageModels;
using PRN222.Ass2.EVDealerSys.BLL.Interfaces;
using PRN222.Ass2.EVDealerSys.Helpers;
using PRN222.Ass2.EVDealerSys.Models.CustomerManagement;
using PRN222.Ass2.EVDealerSys.Hubs;

namespace PRN222.Ass2.EVDealerSys.Pages.CustomerManagement
{
    public class EditModel : BaseCrudPageModel
    {
        private readonly ICustomerService _customerService;
        private readonly IDealerService _dealerService;
        private readonly IHubContext<ManagementHub> _hubContext;

        public EditModel(IActivityLogService logService, ICustomerService customerService, IDealerService dealerService, IHubContext<ManagementHub> hubContext) : base(logService)
        {
            _customerService = customerService;
            _dealerService = dealerService;
            _hubContext = hubContext;
        }

        [BindProperty]
        public EditCustomerViewModel ViewModel { get; set; } = new();
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

                var dealers = await _dealerService.GetAllDealersAsync();

                ViewModel = new EditCustomerViewModel
                {
                    Id = customer.Id,
                    Name = customer.Name ?? "",
                    Email = customer.Email ?? "",
                    Phone = customer.Phone ?? "",
                    Address = customer.Address ?? "",
                    DealerId = customer.DealerId ?? 0,
                    CurrentDealerName = customer.Dealer?.Name ?? "",
                    OrderCount = customer.Orders?.Count ?? 0,
                    AvailableDealers = MappingHelper.ToDealerSelectItems(dealers)
                };
                await LogAsync("Open Edit Customer", $"ID={id}");

                return Page();
            }
            catch (Exception)
            {
                SetError("Có lỗi xảy ra khi tải thông tin chỉnh sửa.");
                return RedirectToAction(nameof(Index));
            }
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    await ReloadEditViewModelAsync(id);
                    return Page();
                }

                // Check if email exists (excluding current customer)
                if (await _customerService.EmailExistsAsync(ViewModel.Email, id))
                {
                    ModelState.AddModelError("ViewModel.Email", "Email này đã được sử dụng bởi khách hàng khác.");
                    await ReloadEditViewModelAsync(id);
                    return Page();
                }

                await _customerService.UpdateCustomerAsync(
                    id,
                    ViewModel.Name,
                    ViewModel.Email,
                    ViewModel.Phone,
                    ViewModel.Address,
                    ViewModel.DealerId
                );

                SetSuccess("Cập nhật thông tin khách hàng thành công!");
                await LogAsync("Edit Customer", $"ID={ViewModel.Id}");
                
                // Send SignalR notification
                await _hubContext.Clients.All.SendAsync("ReceiveCustomerUpdated", new
                {
                    id = ViewModel.Id,
                    name = ViewModel.Name,
                    email = ViewModel.Email,
                    phone = ViewModel.Phone,
                    address = ViewModel.Address
                });
                
                return RedirectToPage("/Customers/Index");
            }
            catch (Exception)
            {
                SetError("Có lỗi xảy ra khi cập nhật thông tin khách hàng.");
                await ReloadEditViewModelAsync(id);
                return Page();
            }
        }
        private async Task ReloadEditViewModelAsync(int id)
        {
            var dealers = await _dealerService.GetAllDealersAsync();
            ViewModel.AvailableDealers = MappingHelper.ToDealerSelectItems(dealers);
            var currentCustomer = await _customerService.GetCustomerByIdAsync(id);
            ViewModel.CurrentDealerName = currentCustomer?.Dealer?.Name ?? "";
            ViewModel.OrderCount = currentCustomer?.Orders?.Count ?? 0;
        }
    }
}
