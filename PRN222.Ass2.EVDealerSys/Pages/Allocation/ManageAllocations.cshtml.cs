using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN222.Ass2.EVDealerSys.BLL.Interfaces;
using PRN222.Ass2.EVDealerSys.BusinessObjects.DTO.Allocation;
using PRN222.Ass2.EVDealerSys.DAL.Interfaces;

namespace PRN222.Ass2.EVDealerSys.Pages.Allocation;

[Authorize(Roles = "1")] // Admin only
public class ManageAllocationsModel : PageModel
{
    private readonly IAllocationService _allocationService;
    private readonly IVehicleAllocationRepository _allocationRepo;

    public List<AllocationRequestDto> AllRequests { get; set; } = new();
    public List<AllocationRequestDto> Requests { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public int? StatusFilter { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? DealerFilter { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? SearchTerm { get; set; }

    public ManageAllocationsModel(IAllocationService allocationService, IVehicleAllocationRepository allocationRepo)
    {
        _allocationService = allocationService;
        _allocationRepo = allocationRepo;
    }

    public async Task OnGetAsync()
    {
        var allAllocations = await _allocationRepo.GetAllWithDetailsAsync();
        AllRequests = allAllocations.Select(MapToDto).ToList();
        Requests = AllRequests;

        // Apply filters
        if (StatusFilter.HasValue)
        {
            Requests = Requests.Where(r => r.Status == StatusFilter.Value).ToList();
        }

        if (!string.IsNullOrEmpty(DealerFilter))
        {
            Requests = Requests.Where(r => r.DealerName == DealerFilter).ToList();
        }

        if (!string.IsNullOrEmpty(SearchTerm))
        {
            Requests = Requests.Where(r =>
                r.VehicleModel?.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) == true ||
                r.VehicleVersion?.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) == true
            ).ToList();
        }
    }

    public async Task<IActionResult> OnPostFulfillAsync(int allocationId)
    {
        var (success, message) = await _allocationService.FulfillAllocationAsync(allocationId);
        TempData[success ? "SuccessMessage" : "ErrorMessage"] = message;
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeliverAsync(int allocationId, DateTime deliveryDate)
    {
        var (success, message) = await _allocationService.UpdateDeliveryStatusAsync(allocationId, deliveryDate);
        TempData[success ? "SuccessMessage" : "ErrorMessage"] = message;
        return RedirectToPage();
    }

    private AllocationRequestDto MapToDto(BusinessObjects.Models.VehicleAllocation allocation)
    {
        return new AllocationRequestDto
        {
            Id = allocation.Id,
            VehicleId = allocation.VehicleId ?? 0,
            VehicleModel = allocation.Vehicle?.Model,
            VehicleVersion = allocation.Vehicle?.Version,
            Quantity = allocation.Quantity ?? 0,
            RequestedColor = allocation.RequestedColor,
            DesiredDeliveryDate = allocation.DesiredDeliveryDate ?? DateTime.Now,
            ReasonText = allocation.Reason,
            ToDealerId = allocation.ToDealerId ?? 0,
            DealerName = allocation.ToDealer?.Name,
            RequestedByUserId = allocation.RequestedByUserId ?? 0,
            RequestedByUserName = allocation.RequestedByUser?.Name,
            RequestDate = allocation.RequestDate ?? DateTime.Now,
            Status = allocation.Status ?? 0,
            StatusText = _allocationService.GetStatusText(allocation.Status ?? 0),
            ApprovedByUserId = allocation.ApprovedByUserId,
            ApprovalNotes = allocation.ApprovalNotes,
            StaffSuggestion = allocation.StaffSuggestion,
            AllocationDate = allocation.AllocationDate,
            ShipmentDate = allocation.ShipmentDate,
            DeliveryDate = allocation.DeliveryDate
        };
    }
}
