using Microsoft.AspNetCore.Mvc;

using PRN222.Ass2.EVDealerSys.Base.BasePageModels;
using PRN222.Ass2.EVDealerSys.BLL.Interfaces;
using PRN222.Ass2.EVDealerSys.Helpers;
using PRN222.Ass2.EVDealerSys.Models;

namespace PRN222.Ass2.EVDealerSys.Pages.DealersManagement;

public class DetailsModel : BaseCrudPageModel
{
    private readonly IDealerService _dealerService;

    public DetailsModel(IActivityLogService logService, IDealerService dealerService) : base(logService)
    {
        _dealerService = dealerService;
    }

    public DealerDetailsViewModel ViewModel { get; set; } = new();
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

            ViewModel = MappingHelper.MapToDealerDetailsViewModel(dealer);

            await LogAsync("View Dealer Details", $"ID={id}");
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
