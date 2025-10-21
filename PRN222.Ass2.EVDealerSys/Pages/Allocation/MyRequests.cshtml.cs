using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN222.Ass2.EVDealerSys.BLL.Interfaces;
using PRN222.Ass2.EVDealerSys.BusinessObjects.DTO.Allocation;
using System.Security.Claims;

namespace PRN222.Ass2.EVDealerSys.Pages.Allocation;

[Authorize(Roles = "2")]
public class MyRequestsModel : PageModel
{
    private readonly IAllocationService _allocationService;

    public List<AllocationRequestDto> Requests { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public int? StatusFilter { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? SearchTerm { get; set; }

    public MyRequestsModel(IAllocationService allocationService)
    {
        _allocationService = allocationService;
    }

    public async Task OnGetAsync()
    {
        var dealerId = int.Parse(User.FindFirstValue("DealerId") ?? "0");
        
        Requests = (await _allocationService.GetDealerRequestsAsync(dealerId)).ToList();

        // Apply filters
        if (StatusFilter.HasValue)
        {
            Requests = Requests.Where(r => r.Status == StatusFilter.Value).ToList();
        }

        if (!string.IsNullOrEmpty(SearchTerm))
        {
            Requests = Requests.Where(r => 
                r.VehicleModel?.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) == true ||
                r.VehicleVersion?.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) == true
            ).ToList();
        }
    }
}
