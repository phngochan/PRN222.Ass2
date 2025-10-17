using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

using PRN222.Ass2.EVDealerSys.Base.BasePageModels;
using PRN222.Ass2.EVDealerSys.BLL.Interfaces;
using PRN222.Ass2.EVDealerSys.Models;
using PRN222.Ass2.EVDealerSys.Hubs;

namespace PRN222.Ass2.EVDealerSys.Pages.DealersManagement;

public class EditModel : BaseCrudPageModel
{
    private readonly IDealerService _dealerService;
    private readonly IHubContext<ManagementHub> _hubContext;

    public EditModel(IActivityLogService logService, IDealerService dealerService, IHubContext<ManagementHub> hubContext) : base(logService)
    {
        _dealerService = dealerService;
        _hubContext = hubContext;
    }

    [BindProperty]
    public EditDealerViewModel ViewModel { get; set; } = new();
    public async Task<IActionResult> OnGetAsync(int id)
    {
        try
        {
            var dealer = await _dealerService.GetDealerByIdAsync(id);
            if (dealer == null)
            {
                SetError("Không tìm thấy đại lý.");
                return RedirectToAction(nameof(Index));
            }

            ViewModel = new EditDealerViewModel
            {
                Id = dealer.Id,
                Name = dealer.Name ?? "",
                Address = dealer.Address ?? "",
                Region = dealer.Region ?? "",
                CustomerCount = dealer.Customers?.Count ?? 0,
                InventoryCount = dealer.Inventories?.Count ?? 0,
                OrderCount = dealer.Orders?.Count ?? 0,
                UserCount = dealer.Users?.Count ?? 0
            };
            await LogAsync("Open Edit Dealer", $"ID={id}");

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

            if (await _dealerService.NameExistsAsync(ViewModel.Name, ViewModel.Id))
            {
                ModelState.AddModelError("ViewModel.Email", "Email này đã được sử dụng bởi đại lý khác.");
                await ReloadEditViewModelAsync(id);
                return Page();
            }

            await _dealerService.UpdateDealerAsync(ViewModel.Id, ViewModel.Name, ViewModel.Address, ViewModel.Region);

            SetSuccess("Cập nhật thông tin đại lý thành công!");
            await LogAsync("Edit Dealer", $"ID={ViewModel.Id}");
            
            // Send SignalR notification
            await _hubContext.Clients.All.SendAsync("ReceiveDealerUpdated", new
            {
                id = ViewModel.Id,
                name = ViewModel.Name,
                address = ViewModel.Address,
                region = ViewModel.Region
            });
            
            return RedirectToPage(nameof(Index));
        }
        catch (Exception)
        {
            SetError("Có lỗi xảy ra khi cập nhật thông tin đại lý.");
            await ReloadEditViewModelAsync(id);
            return Page();
        }
    }
    private async Task ReloadEditViewModelAsync(int id)
    {
        var existingDealer = await _dealerService.GetDealerByIdAsync(id);
        if (existingDealer != null)
        {
            ViewModel.CustomerCount = existingDealer.Customers?.Count ?? 0;
            ViewModel.InventoryCount = existingDealer.Inventories?.Count ?? 0;
            ViewModel.OrderCount = existingDealer.Orders?.Count ?? 0;
            ViewModel.UserCount = existingDealer.Users?.Count ?? 0;
        }
    }
}
