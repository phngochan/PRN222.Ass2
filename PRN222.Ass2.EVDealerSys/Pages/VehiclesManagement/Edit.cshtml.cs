using EVDealerSys.Models;
using Microsoft.AspNetCore.SignalR;
using PRN222.Ass2.EVDealerSys.Hubs;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

using PRN222.Ass2.EVDealerSys.Base.BasePageModels;
using PRN222.Ass2.EVDealerSys.BLL.Interfaces;

namespace PRN222.Ass2.EVDealerSys.Pages.VehiclesManagement
{
    public class EditModel : BaseCrudPageModel
    {
        private readonly IVehicleService _vehicleService;
        private readonly IHubContext<VehicleHub> _hubContext;

        public EditModel(IVehicleService vehicleService, IActivityLogService logService, IHubContext<VehicleHub> hubContext) : base(logService)
        {
            _vehicleService = vehicleService;
            _hubContext = hubContext;
        }

        [BindProperty]
        public EditVehicleViewModel Input { get; set; } = new();

        public List<SelectListItem> ModelOptions { get; set; } = new();
        public List<SelectListItem> ColorOptions { get; set; } = new();
        public List<SelectListItem> StatusOptions { get; set; } = new();

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

                Input = new EditVehicleViewModel
                {
                    Id = vehicle.Id,
                    VehicleModel = vehicle.Model ?? string.Empty,
                    Version = vehicle.Version ?? string.Empty,
                    Color = vehicle.Color ?? string.Empty,
                    Config = vehicle.Config,
                    Price = vehicle.Price ?? 0,
                    Status = vehicle.Status ?? 1,
                    InventoryCount = vehicle.Inventories?.Count ?? 0,
                    OrderCount = vehicle.OrderItems?.Count ?? 0,
                    AllocationCount = vehicle.VehicleAllocations?.Count ?? 0
                };

                await LoadDropdownsAsync();
                await LogAsync("View Edit Vehicle", $"Vehicle ID: {id}");
                return Page();
            }
            catch (Exception ex)
            {
                SetError("Có lỗi xảy ra khi tải thông tin xe");
                await LogAsync("Error", ex.Message);
                return RedirectToPage("./Index");
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                SetError("Vui lòng kiểm tra lại thông tin nhập.");
                await LoadDropdownsAsync();
                return Page();
            }

            try
            {
                var existingVehicle = await _vehicleService.GetVehicleByIdAsync(Input.Id);
                if (existingVehicle == null)
                {
                    SetError("Không tìm thấy xe để cập nhật");
                    return RedirectToPage("./Index");
                }

                // Update vehicle properties
                existingVehicle.Model = Input.VehicleModel;
                existingVehicle.Version = Input.Version;
                existingVehicle.Color = Input.Color;
                existingVehicle.Config = Input.Config;
                existingVehicle.Price = Input.Price;
                existingVehicle.Status = Input.Status;

                var updatedVehicle = await _vehicleService.UpdateVehicleAsync(existingVehicle);

                SetSuccess($"Xe {Input.VehicleModel} {Input.Version} đã được cập nhật thành công!");
                await LogAsync("Update Vehicle", $"Updated vehicle ID: {Input.Id}");

                // Gửi thông báo SignalR cho tất cả client
                await _hubContext.Clients.All.SendAsync("VehicleUpdated", updatedVehicle.Id, updatedVehicle.Model);

                return RedirectToPage("./Details", new { id = Input.Id });
            }
            catch (InvalidOperationException ex)
            {
                SetError(ex.Message);
                await LogAsync("Error", ex.Message);
                await LoadDropdownsAsync();
                return Page();
            }
            catch (Exception ex)
            {
                SetError("Có lỗi xảy ra khi cập nhật xe: " + ex.Message);
                await LogAsync("Error", ex.Message);
                await LoadDropdownsAsync();
                return Page();
            }
        }

        private async Task LoadDropdownsAsync()
        {
            ModelOptions = await GetModelSelectList();
            ColorOptions = await GetColorSelectList();
            StatusOptions = GetStatusSelectList();
        }

        private async Task<List<SelectListItem>> GetModelSelectList()
        {
            var models = new List<SelectListItem>
            {
                new SelectListItem { Value = "", Text = "-- Chọn Model --" }
            };
            try
            {
                var modelList = await _vehicleService.GetDistinctModelsAsync();
                models.AddRange(modelList.Select(m => new SelectListItem
                {
                    Value = m,
                    Text = m
                }));
            }
            catch { }
            return models;
        }

        private async Task<List<SelectListItem>> GetColorSelectList()
        {
            var colors = new List<SelectListItem>
            {
                new SelectListItem { Value = "", Text = "-- Chọn màu --" }
            };
            var predefinedColors = new[] { "Trắng", "Đen", "Xám", "Xanh", "Đỏ", "Bạc" };
            colors.AddRange(predefinedColors.Select(c => new SelectListItem { Value = c, Text = c }));

            try
            {
                var colorList = await _vehicleService.GetDistinctColorsAsync();
                var additionalColors = colorList.Where(c => !predefinedColors.Contains(c));
                colors.AddRange(additionalColors.Select(c => new SelectListItem { Value = c, Text = c }));
            }
            catch { }
            return colors;
        }

        private List<SelectListItem> GetStatusSelectList()
        {
            return new List<SelectListItem>
            {
                new SelectListItem { Value = "1", Text = "Có sẵn" },
                new SelectListItem { Value = "2", Text = "Đã bán" },
                new SelectListItem { Value = "3", Text = "Bảo trì" },
                new SelectListItem { Value = "4", Text = "Đặt trước" }
            };
        }

    }

}
