using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

using PRN222.Ass2.EVDealerSys.Base.BasePageModels;
using PRN222.Ass2.EVDealerSys.BLL.Interfaces;
using PRN222.Ass2.EVDealerSys.Helpers;
using PRN222.Ass2.EVDealerSys.Models.CustomerManagement;
using PRN222.Ass2.EVDealerSys.Hubs;

namespace PRN222.Ass2.EVDealerSys.Pages.CustomerManagement
{
    public class CreateModel : BaseCrudPageModel
    {
        private readonly ICustomerService _customerService;
        private readonly IDealerService _dealerService;
        private readonly IHubContext<ManagementHub> _hubContext;

        public CreateModel(IActivityLogService logService, ICustomerService customerService, IDealerService dealerService, IHubContext<ManagementHub> hubContext) : base(logService)
        {
            _customerService = customerService;
            _dealerService = dealerService;
            _hubContext = hubContext;
        }
        [BindProperty]
        public CreateCustomerViewModel ViewModel { get; set; } = new();
        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                var dealers = await _dealerService.GetAllDealersAsync();
                ViewModel.AvailableDealers = MappingHelper.ToDealerSelectItems(dealers);
                await LogAsync("Open Create Customer", $"Tạo khách hàng: {ViewModel.Name} ({ViewModel.Email})");

                return Page();
            }
            catch (Exception)
            {
                SetError("Có lỗi xảy ra khi tải thông tin.");
                return RedirectToAction(nameof(Index));
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    await ReloadCreateViewModelAsync();
                    return Page();
                }

                // Check if email exists (excluding current customer)
                if (await _customerService.EmailExistsAsync(ViewModel.Email))
                {
                    ModelState.AddModelError("ViewModel.Email", "Email này đã được sử dụng bởi khách hàng khác.");
                    await ReloadCreateViewModelAsync();
                    return Page();
                }

                await _customerService.CreateCustomerAsync(
                    ViewModel.Name, ViewModel.Email, ViewModel.Phone, ViewModel.Address, ViewModel.DealerId
                );

                SetSuccess("Thêm thông tin khách hàng thành công!");
                await LogAsync("Create Customer", $"Tạo khách hàng: {ViewModel.Name} ({ViewModel.Email})");
                
                // Send SignalR notification
                await _hubContext.Clients.All.SendAsync("ReceiveCustomerCreated", new
                {
                    name = ViewModel.Name,
                    email = ViewModel.Email,
                    phone = ViewModel.Phone,
                    address = ViewModel.Address
                });
                
                return RedirectToPage(nameof(Index));
            }
            catch (Exception)
            {
                SetError("Có lỗi xảy ra khi tạo thông tin khách hàng.");
                await ReloadCreateViewModelAsync();
                return Page();
            }
        }
        private async Task ReloadCreateViewModelAsync()
        {
            var dealers = await _dealerService.GetAllDealersAsync();
            ViewModel.AvailableDealers = MappingHelper.ToDealerSelectItems(dealers);
        }
    }
}
