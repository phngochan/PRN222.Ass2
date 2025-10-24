using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN222.Ass2.EVDealerSys.BLL.Interfaces;
using PRN222.Ass2.EVDealerSys.BusinessObjects.DTO.Allocation;
using System.Security.Claims;

namespace PRN222.Ass2.EVDealerSys.Pages.Allocation;

[Authorize(Roles = "2")] // Chỉ Role 2: Dealer Manager
public class ManagerForwardedRequestsModel : PageModel
{
    private readonly IAllocationService _allocationService;

    public List<AllocationRequestDto> ForwardedRequests { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public int? StatusFilter { get; set; }

    public ManagerForwardedRequestsModel(IAllocationService allocationService)
    {
        _allocationService = allocationService;
    }

    public async Task OnGetAsync()
    {
        var dealerId = int.Parse(User.FindFirstValue("DealerId") ?? "0");
        
        // Lấy danh sách yêu cầu ĐÃ GỬI LÊN Role 4
        ForwardedRequests = (await _allocationService.GetManagerForwardedRequestsAsync(dealerId)).ToList();

        // Apply filters
        if (StatusFilter.HasValue)
        {
            ForwardedRequests = ForwardedRequests.Where(r => r.Status == StatusFilter.Value).ToList();
        }

        // Check stock cho mỗi yêu cầu
        foreach (var request in ForwardedRequests)
        {
            var (availableStock, isSufficient) = await _allocationService.CheckStockAvailabilityAsync(
                request.VehicleId, 
                request.Quantity, 
                request.RequestedColor);

            request.AvailableStock = availableStock;
            request.IsStockSufficient = isSufficient;
        }
    }
}
