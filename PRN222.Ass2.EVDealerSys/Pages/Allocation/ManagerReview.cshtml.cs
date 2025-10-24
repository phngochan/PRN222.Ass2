using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN222.Ass2.EVDealerSys.BLL.Interfaces;
using PRN222.Ass2.EVDealerSys.BusinessObjects.DTO.Allocation;
using System.Security.Claims;

namespace PRN222.Ass2.EVDealerSys.Pages.Allocation;

[Authorize(Roles = "2")] // Role 2: Dealer Manager
public class ManagerReviewModel : PageModel
{
    private readonly IAllocationService _allocationService;

    public List<AllocationRequestDto> PendingRequests { get; set; } = new();
    public List<AllocationRequestDto> AllRequests { get; set; } = new();
    public List<AllocationRequestDto> DeliveredRequests { get; set; } = new();  // NEW

    [BindProperty(SupportsGet = true)]
    public string ActiveTab { get; set; } = "pending";

    public ManagerReviewModel(IAllocationService allocationService)
    {
        _allocationService = allocationService;
    }

    public async Task OnGetAsync()
    {
        var dealerId = int.Parse(User.FindFirstValue("DealerId") ?? "0");

        // Lấy yêu cầu chờ Manager xét duyệt (Status = 0: PendingManagerReview)
        PendingRequests = (await _allocationService.GetPendingManagerReviewAsync(dealerId)).ToList();

        // Lấy tất cả yêu cầu của dealer
        AllRequests = (await _allocationService.GetDealerRequestsAsync(dealerId)).ToList();

        // NEW: Lấy các yêu cầu đã giao hàng chờ xác nhận (Status = 5: Delivered)
        DeliveredRequests = AllRequests.Where(r => r.Status == 5).ToList();

        // Check stock cho mỗi yêu cầu
        foreach (var request in PendingRequests)
        {
            var (availableStock, isSufficient) = await _allocationService.CheckStockAvailabilityAsync(
                request.VehicleId, 
                request.Quantity, 
                request.RequestedColor);

            request.AvailableStock = availableStock;
            request.IsStockSufficient = isSufficient;
        }
    }

    public async Task<IActionResult> OnPostApproveAsync(int allocationId, string? notes)
    {
        var managerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        
        var (success, message) = await _allocationService.ManagerApproveAndForwardAsync(
            allocationId, managerId, notes);

        TempData[success ? "SuccessMessage" : "ErrorMessage"] = message;
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostRejectAsync(int allocationId, string reason)
    {
        var managerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        
        var (success, message) = await _allocationService.ManagerRejectAsync(
            allocationId, managerId, reason);

        TempData[success ? "SuccessMessage" : "ErrorMessage"] = message;
        return RedirectToPage();
    }

    // NEW: Handler xác nhận đã nhận hàng
    public async Task<IActionResult> OnPostConfirmReceivedAsync(int allocationId)
    {
        var managerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        
        var (success, message) = await _allocationService.ConfirmReceivedAsync(allocationId, managerId);

        TempData[success ? "SuccessMessage" : "ErrorMessage"] = message;
        return RedirectToPage(new { ActiveTab = "delivered" });
    }
}
