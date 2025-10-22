using EVDealerSys.Models;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

using PRN222.Ass2.EVDealerSys.Base.BasePageModels;
using PRN222.Ass2.EVDealerSys.BLL.Interfaces;
using PRN222.Ass2.EVDealerSys.Hubs;

namespace PRN222.Ass2.EVDealerSys.Pages.VehiclesManagement
{
    [Authorize(Roles = "1,2")]
    public class DeleteModel : BaseCrudPageModel
    {
        private readonly IVehicleService _vehicleService;
        private readonly IHubContext<VehicleHub> _hubContext;

        public DeleteModel(IVehicleService vehicleService, IActivityLogService logService, IHubContext<VehicleHub> hubContext, IHubContext<ActivityLogHub> activityLogHubContext) : base(logService)
        {
            _vehicleService = vehicleService;
            _hubContext = hubContext;
            SetActivityLogHubContext(activityLogHubContext);
        }

        [BindProperty]
        public DeleteVehicleViewModel Input { get; set; } = new();

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

                Input = new DeleteVehicleViewModel
                {
                    Id = vehicle.Id,
                    VehicleModel = vehicle.Model ?? string.Empty,
                    Version = vehicle.Version ?? string.Empty,
                    Color = vehicle.Color ?? string.Empty,
                    Config = vehicle.Config,
                    Price = vehicle.Price ?? 0,
                    Status = vehicle.Status ?? 1,
                    TotalInventory = vehicle.Inventories?.Count ?? 0,
                    TotalOrders = vehicle.OrderItems?.Count ?? 0,
                    TotalAllocations = vehicle.VehicleAllocations?.Count ?? 0
                };

                await LogAsync("View Delete Vehicle", $"Vehicle ID: {id}");
                return Page();
            }
            catch (Exception ex)
            {
                SetError(ex.Message);
                await LogAsync("Error", ex.Message);
                return RedirectToPage("./Index");
            }
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            try
            {
                var vehicle = await _vehicleService.GetVehicleByIdAsync(Input.Id);
                if (vehicle == null)
                {
                    SetError("Không tìm thấy xe để xóa");
                    return RedirectToPage("./Index");
                }

                // Check if vehicle has related data
                bool hasInventory = vehicle.Inventories?.Any() == true;
                bool hasOrders = vehicle.OrderItems?.Any() == true;
                bool hasAllocations = vehicle.VehicleAllocations?.Any() == true;

                if (hasInventory || hasOrders || hasAllocations)
                {
                    var relatedData = new List<string>();
                    if (hasInventory) relatedData.Add("tồn kho");
                    if (hasOrders) relatedData.Add("đơn hàng");
                    if (hasAllocations) relatedData.Add("phân bổ");

                    SetError($"Không thể xóa xe vì còn dữ liệu liên quan: {string.Join(", ", relatedData)}");
                    return Page();
                }

                await _vehicleService.DeleteVehicleAsync(Input.Id);

                SetSuccess($"Xe {vehicle.Model} {vehicle.Version} đã được xóa thành công!");
                await LogAsync("Delete Vehicle", $"Deleted vehicle ID: {Input.Id}");

                // Gửi thông báo SignalR cho tất cả client
                await _hubContext.Clients.All.SendAsync("VehicleDeleted", Input.Id);

                return RedirectToPage("./Index");
            }
            catch (InvalidOperationException ex)
            {
                SetError(ex.Message);
                await LogAsync("Error", ex.Message);
                return Page();
            }
            catch (Exception ex)
            {
                SetError("Có lỗi xảy ra khi xóa xe: " + ex.Message);
                await LogAsync("Error", ex.Message);
                return Page();
            }
        }

    }
}
