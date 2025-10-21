using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

using PRN222.Ass2.EVDealerSys.Base.BasePageModels;
using PRN222.Ass2.EVDealerSys.BLL.Interfaces;
using PRN222.Ass2.EVDealerSys.Helpers;
using PRN222.Ass2.EVDealerSys.Hubs;
using PRN222.Ass2.EVDealerSys.Models;

namespace PRN222.Ass2.EVDealerSys.Pages.DealersManagement;

[Authorize(Roles = "1,2")]
public class IndexModel : BaseCrudPageModel
{
    private readonly IDealerService _dealerService;

    public IndexModel(IActivityLogService activityLogService, IDealerService dealerService, IHubContext<ActivityLogHub> activityLogHubContext) : base(activityLogService)
    {
        _dealerService = dealerService;
        SetActivityLogHubContext(activityLogHubContext);
    }

    public DealerManagementViewModel ViewModel { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public int CurrentPage { get; set; } = 1;

    [BindProperty(SupportsGet = true)]
    public string? SearchName { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? SearchAddress { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? SelectedRegion { get; set; }
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
        var (dealers, totalCount, totalPages) = await _dealerService.GetPagedDealersAsync(
                CurrentPage, 10, SearchName, SelectedRegion, SearchAddress);

        var regions = await _dealerService.GetDistinctRegionsAsync();

        ViewModel = new DealerManagementViewModel
        {
            Dealers = MappingHelper.ToDealerListItem(dealers),
            TotalDealers = await _dealerService.GetTotalDealersCountAsync(),
            TotalCustomers = await _dealerService.GetTotalCustomersCountAsync(),
            TotalOrders = await _dealerService.GetTotalOrdersCountAsync(),
            TotalRegions = regions.Count(),
            CurrentPage = CurrentPage,
            TotalCount = totalCount,
            TotalPages = totalPages,
            SearchName = SearchName,
            SearchAddress = SearchAddress,
            SelectedRegion = SelectedRegion,
            AvailableRegions = regions.ToList()
        };

        await LogAsync("View Dealer List", $"SearchName={SearchName}, SelectedRegion={SelectedRegion}, SearchAddress={SearchAddress}");
    }
}
