using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN222.Ass2.EVDealerSys.BLL.Interfaces;
using PRN222.Ass2.EVDealerSys.BusinessObjects.DTO.Allocation;
using System.Security.Claims;

namespace PRN222.Ass2.EVDealerSys.Pages.Allocation;

[Authorize(Roles = "2")] // Manager only
public class PendingApprovalsModel : PageModel
{
    private readonly IAllocationService _allocationService;

    public List<AllocationRequestDto> PendingRequests { get; set; } = new();

    public PendingApprovalsModel(IAllocationService allocationService)
    {
        _allocationService = allocationService;
    }

    public async Task OnGetAsync()
    {
        PendingRequests = (await _allocationService.GetPendingRequestsAsync()).ToList();

        // Check stock for each request
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

    public async Task<IActionResult> OnPostApproveAsync(int allocationId, string? notes, string? suggestion)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        
        var (success, message) = await _allocationService.ApproveRequestAsync(allocationId, userId, notes, suggestion);

        TempData[success ? "SuccessMessage" : "ErrorMessage"] = message;
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostRejectAsync(int allocationId, string reason)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        
        var (success, message) = await _allocationService.RejectRequestAsync(allocationId, userId, reason);

        TempData[success ? "SuccessMessage" : "ErrorMessage"] = message;
        return RedirectToPage();
    }
}
