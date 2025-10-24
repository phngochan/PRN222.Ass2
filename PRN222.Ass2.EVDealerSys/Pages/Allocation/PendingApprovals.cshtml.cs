using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN222.Ass2.EVDealerSys.BLL.Interfaces;
using PRN222.Ass2.EVDealerSys.BusinessObjects.DTO.Allocation;
using System.Security.Claims;

namespace PRN222.Ass2.EVDealerSys.Pages.Allocation;

[Authorize(Roles = "4")] // Role 4: EVM Staff
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
        // Lấy yêu cầu chờ EVM phê duyệt (Status = 1: PendingEVMApproval)
        // Đây là các yêu cầu đã được Manager (Role 2) xét duyệt
        PendingRequests = (await _allocationService.GetPendingEVMApprovalAsync()).ToList();

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
        
        var (success, message) = await _allocationService.EVMApproveRequestAsync(
            allocationId, userId, notes, suggestion);

        TempData[success ? "SuccessMessage" : "ErrorMessage"] = message;
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostRejectAsync(int allocationId, string reason)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        
        var (success, message) = await _allocationService.EVMRejectRequestAsync(
            allocationId, userId, reason);

        TempData[success ? "SuccessMessage" : "ErrorMessage"] = message;
        return RedirectToPage();
    }
}
