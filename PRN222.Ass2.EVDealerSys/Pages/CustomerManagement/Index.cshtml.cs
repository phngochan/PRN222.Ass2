using Microsoft.AspNetCore.Mvc;

using PRN222.Ass2.EVDealerSys.Base.BasePageModels;
using PRN222.Ass2.EVDealerSys.BLL.Interfaces;
using PRN222.Ass2.EVDealerSys.Helpers;
using PRN222.Ass2.EVDealerSys.Models.CustomerManagement;

namespace PRN222.Ass2.EVDealerSys.Pages.CustomerManagement
{
    public class IndexModel : BaseCrudPageModel
    {
        private readonly ICustomerService _customerService;
        private readonly IDealerService _dealerService;
        public IndexModel(ICustomerService customerService, IDealerService dealerService, IActivityLogService logService) : base(logService)
        {
            _customerService = customerService;
            _dealerService = dealerService;
        }
        public CustomerManagementViewModel ViewModel { get; set; } = new();
        [BindProperty(SupportsGet = true)] public int PageNumber { get; set; } = 1;
        [BindProperty(SupportsGet = true)] public string? SearchName { get; set; }
        [BindProperty(SupportsGet = true)] public string? SearchEmail { get; set; }
        [BindProperty(SupportsGet = true)] public string? SearchPhone { get; set; }
        [BindProperty(SupportsGet = true)] public int? DealerId { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                await LoadListAsync();
                return Page();
            }
            catch (Exception ex)
            {
                SetError("Đã xảy ra lỗi khi tải dữ liệu.");
                await LogAsync("Error", ex.Message);
                return Page();
            }
        }
        private async Task LoadListAsync()
        {
            var (customers, totalCount, totalPages) = await _customerService.GetPagedCustomersAsync(
                PageNumber, 10, SearchName, SearchEmail, SearchPhone, DealerId);

            var dealers = await _dealerService.GetAllDealersAsync();

            ViewModel = new CustomerManagementViewModel
            {
                Customers = MappingHelper.ToCustomerListItems(customers),
                TotalCustomers = await _customerService.GetTotalCustomersCountAsync(),
                CustomersWithOrders = await _customerService.GetCustomersWithOrdersCountAsync(),
                TotalOrders = await _customerService.GetTotalOrdersCountAsync(),
                NewCustomersThisMonth = await _customerService.GetNewCustomersThisMonthCountAsync(),
                CurrentPage = PageNumber,
                TotalPages = totalPages,
                TotalCount = totalCount,
                SearchName = SearchName,
                SearchEmail = SearchEmail,
                SearchPhone = SearchPhone,
                SelectedDealerId = DealerId,
                AvailableDealers = MappingHelper.ToDealerSelectItems(dealers)
            };

            await LogAsync("View Customer List", $"SearchName={SearchName}, DealerId={DealerId}");
        }
    }
}
