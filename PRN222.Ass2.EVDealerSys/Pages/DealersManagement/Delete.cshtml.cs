using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

using PRN222.Ass2.EVDealerSys.Base.BasePageModels;
using PRN222.Ass2.EVDealerSys.BLL.Interfaces;
using PRN222.Ass2.EVDealerSys.Models;
using PRN222.Ass2.EVDealerSys.Hubs;

namespace PRN222.Ass2.EVDealerSys.Pages.DealersManagement;

public class DeleteModel : BaseCrudPageModel
{
    private readonly IDealerService _dealerService;
    private readonly IHubContext<ManagementHub> _hubContext;

    public DeleteModel(IActivityLogService logService, IDealerService dealerService, IHubContext<ManagementHub> hubContext) : base(logService)
    {
        _dealerService = dealerService;
        _hubContext = hubContext;
    }

    [BindProperty]
    public DeleteDealerViewModel ViewModel { get; set; } = new();
    public async Task<IActionResult> OnGetAsync(int id)
    {
        try
        {
            var dealer = await _dealerService.GetDealerByIdAsync(id);
            if (dealer == null)
            {
                SetError("Không tìm thấy đại lý.");
                return RedirectToPage("./Index");
            }

            ViewModel = new DeleteDealerViewModel
            {
                Id = dealer.Id,
                Name = dealer.Name ?? "",
                Address = dealer.Address ?? "",
                Region = dealer.Region ?? "",
                CustomerCount = dealer.Customers?.Count ?? 0,
                InventoryCount = dealer.Inventories?.Count ?? 0,
                OrderCount = dealer.Orders?.Count ?? 0,
                UserCount = dealer.Users?.Count ?? 0,
                AllocationCount = dealer.VehicleAllocations?.Count ?? 0
            };

            await LogAsync("View Delete Dealer", $"ID={id}");
            return Page();
        }
        catch (Exception ex)
        {
            SetError("Đã xảy ra lỗi khi tải dữ liệu.");
            await LogAsync("Error", ex.Message);
            return RedirectToPage("./Index");
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        try
        {
            var success = await _dealerService.DeleteDealerAsync(ViewModel.Id);
            if (success)
            {
                SetSuccess("Xóa đại lý thành công!");
                await LogAsync("Delete Dealer", $"Deleted: {ViewModel.Name} (ID={ViewModel.Id})");
                
                // Send SignalR notification
                await _hubContext.Clients.All.SendAsync("ReceiveDealerDeleted", new
                {
                    id = ViewModel.Id,
                    name = ViewModel.Name
                });
            }
            else
            {
                SetError("Không thể xóa đại lý.");
            }
            return RedirectToPage("./Index");
        }
        catch (Exception ex)
        {
            SetError("Có lỗi xảy ra khi xóa đại lý. Có thể đại lý này đang có dữ liệu liên quan.");
            await LogAsync("Error", ex.Message);
            return RedirectToPage("./Index");
        }
    }
}
