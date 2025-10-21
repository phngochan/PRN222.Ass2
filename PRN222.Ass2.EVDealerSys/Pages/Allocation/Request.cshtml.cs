using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using PRN222.Ass2.EVDealerSys.BLL.Interfaces;
using PRN222.Ass2.EVDealerSys.BusinessObjects.DTO.Allocation;
using PRN222.Ass2.EVDealerSys.BusinessObjects.Models;

using System.Security.Claims;

namespace PRN222.Ass2.EVDealerSys.Pages.Allocation;

[Authorize(Roles = "1")] // Admin only
public class RequestModel : PageModel
{
    private readonly IAllocationService _allocationService;
    private readonly IVehicleService _vehicleService;

    [BindProperty]
    public AllocationRequestDto Input { get; set; } = new();

    public List<Vehicle> Vehicles { get; set; } = new();

    public RequestModel(IAllocationService allocationService, IVehicleService vehicleService)
    {
        _allocationService = allocationService;
        _vehicleService = vehicleService;
    }

    public async Task OnGetAsync()
    {
        Vehicles = (await _vehicleService.GetAllVehiclesActiveAsync()).ToList();
    }

    public async Task<IActionResult> OnGetCheckStockAsync(int vehicleId)
    {
        var (availableStock, _) = await _allocationService.CheckStockAvailabilityAsync(vehicleId, 0, null);
        return new JsonResult(new { availableStock });
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            Vehicles = (await _vehicleService.GetAllVehiclesActiveAsync()).ToList();
            return Page();
        }

        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        var dealerId = int.Parse(User.FindFirstValue("DealerId") ?? "0");

        Input.RequestedByUserId = userId;
        Input.ToDealerId = dealerId;
        Input.RequestDate = DateTime.Now;

        var (success, message, allocation) = await _allocationService.CreateRequestAsync(Input);

        if (success)
        {
            TempData["SuccessMessage"] = message;
            return RedirectToPage("./MyRequests");
        }

        TempData["ErrorMessage"] = message;
        Vehicles = (await _vehicleService.GetAllVehiclesActiveAsync()).ToList();
        return Page();
    }
}
