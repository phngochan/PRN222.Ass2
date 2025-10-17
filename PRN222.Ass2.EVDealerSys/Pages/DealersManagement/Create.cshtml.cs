using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

using PRN222.Ass2.EVDealerSys.Base.BasePageModels;
using PRN222.Ass2.EVDealerSys.BLL.Interfaces;
using PRN222.Ass2.EVDealerSys.Hubs;
using PRN222.Ass2.EVDealerSys.Models;

namespace PRN222.Ass2.EVDealerSys.Pages.DealersManagement;

public class CreateModel : BaseCrudPageModel
{
    private readonly IDealerService _dealerService;
    private readonly IHubContext<ManagementHub> _hubContext;

    public CreateModel(IActivityLogService logService, IDealerService dealerService, IHubContext<ManagementHub> hubContext) : base(logService)
    {
        _dealerService = dealerService;
        _hubContext = hubContext;
    }

    [BindProperty]
    public CreateDealerViewModel ViewModel { get; set; } = new();

    public async Task<IActionResult> OnPostAsync()
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            if (await _dealerService.NameExistsAsync(ViewModel.Name))
            {
                ModelState.AddModelError("ViewModel.Name", "Tên đại lý này đã tồn tại.");
                return Page();
            }
            var dealerCreated = await _dealerService.CreateDealerAsync(ViewModel.Name, ViewModel.Address, ViewModel.Region);

            SetSuccess("Thêm thông tin đại lý thành công!");
            await LogAsync("Create Dealer", $"Tạo đại lý: {ViewModel.Name} ({ViewModel.Address})");

            // Send SignalR notification
            await _hubContext.Clients.All.SendAsync("ReceiveDealerCreated", new
            {
                id = dealerCreated.Id,
                name = ViewModel.Name,
                address = ViewModel.Address,
                region = ViewModel.Region
            });

            return RedirectToPage(nameof(Index));
        }
        catch (Exception)
        {
            SetError("Có lỗi xảy ra khi tạo thông tin đại lý.");
            return Page();
        }
    }
}
