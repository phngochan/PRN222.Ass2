using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN222.Ass2.EVDealerSys.BLL.Interfaces;
using PRN222.Ass2.EVDealerSys.BusinessObjects.DTO.Allocation;
using PRN222.Ass2.EVDealerSys.BusinessObjects.Models;
using System.Security.Claims;

namespace PRN222.Ass2.EVDealerSys.Pages.Allocation;

[Authorize(Roles = "3")] // Role 3: Dealer Staff
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

    public async Task<IActionResult> OnGetAsync()
    {
        // Kiểm tra DealerId
        var dealerIdClaim = User.FindFirstValue("DealerId");
        if (string.IsNullOrEmpty(dealerIdClaim))
        {
            TempData["ErrorMessage"] = "Tài khoản của bạn chưa được gán vào Dealer. Vui lòng liên hệ Admin.";
            return RedirectToPage("/Dashboard/Index");
        }

        Vehicles = (await _vehicleService.GetAllVehiclesActiveAsync()).ToList();
        return Page();
    }

    public async Task<IActionResult> OnGetCheckStockAsync(int vehicleId)
    {
        var (availableStock, _) = await _allocationService.CheckStockAvailabilityAsync(vehicleId, 0, null);
        return new JsonResult(new { availableStock });
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        var dealerIdClaim = User.FindFirstValue("DealerId");

        // Validation: Kiểm tra DealerId
        if (string.IsNullOrEmpty(dealerIdClaim))
        {
            TempData["ErrorMessage"] = "Lỗi: Dealer ID không hợp lệ. Vui lòng đăng nhập lại hoặc liên hệ Admin.";
            Vehicles = (await _vehicleService.GetAllVehiclesActiveAsync()).ToList();
            return Page();
        }

        var dealerId = int.Parse(dealerIdClaim);

        if (!ModelState.IsValid)
        {
            Vehicles = (await _vehicleService.GetAllVehiclesActiveAsync()).ToList();
            return Page();
        }

        Input.RequestedByUserId = userId;
        Input.ToDealerId = dealerId;
        Input.RequestDate = DateTime.Now;

        // Sử dụng CreateStaffRequestAsync
        var (success, message, allocation) = await _allocationService.CreateStaffRequestAsync(Input);

        if (success)
        {
            TempData["SuccessMessage"] = message;
            return RedirectToPage("./MyRequests");
        }

        TempData["ErrorMessage"] = message;
        Vehicles = (await _vehicleService.GetAllVehiclesActiveAsync()).ToList();
        return Page();    }}
