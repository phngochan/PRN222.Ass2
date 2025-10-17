using EVDealerSys.Models;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using PRN222.Ass2.EVDealerSys.Base.BasePageModels;
using PRN222.Ass2.EVDealerSys.BLL.Interfaces;

namespace PRN222.Ass2.EVDealerSys.Pages.VehiclesManagement
{
    [Authorize(Roles = "1,2")]
    public class DetailsModel : BaseCrudPageModel
    {
        private readonly IVehicleService _vehicleService;

        public DetailsModel(IVehicleService vehicleService, IActivityLogService logService) : base(logService)
        {
            _vehicleService = vehicleService;
        }

        public VehicleDetailsViewModel Vehicle { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                SetError("ID xe không hợp lệ");
                return RedirectToPage("./Index");
            }

            try
            {
                var vehicle = await _vehicleService.GetVehicleByIdAsync(id.Value);
                if (vehicle == null)
                {
                    SetError("Không tìm thấy xe với ID này");
                    return RedirectToPage("./Index");
                }

                Vehicle = new VehicleDetailsViewModel
                {
                    Id = vehicle.Id,
                    Model = vehicle.Model ?? string.Empty,
                    Version = vehicle.Version ?? string.Empty,
                    Color = vehicle.Color ?? string.Empty,
                    Config = vehicle.Config,
                    Price = vehicle.Price ?? 0,
                    Status = vehicle.Status ?? 1,
                    TotalInventory = vehicle.Inventories?.Count ?? 0,
                    TotalOrders = vehicle.OrderItems?.Count ?? 0,
                    TotalAllocations = vehicle.VehicleAllocations?.Count ?? 0,
                };

                await LogAsync("View Vehicle Details", $"Vehicle ID: {id}");
                return Page();
            }
            catch (Exception ex)
            {
                SetError(ex.Message);
                await LogAsync("Error", ex.Message);
                return RedirectToPage("./Index");
            }
        }
    }
}
