using EVDealerSys.Models;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.SignalR;

using PRN222.Ass2.EVDealerSys.Base.BasePageModels;
using PRN222.Ass2.EVDealerSys.BLL.Interfaces;
using PRN222.Ass2.EVDealerSys.BusinessObjects.Models;
using PRN222.Ass2.EVDealerSys.Hubs;

namespace PRN222.Ass2.EVDealerSys.Pages.VehiclesManagement
{
    [Authorize(Roles = "1,2")]
    public class CreateModel : BaseCrudPageModel
    {
        private readonly IVehicleService _vehicleService;
        private readonly IHubContext<VehicleHub> _hubContext;

        public CreateModel(IVehicleService vehicleService, IActivityLogService logService, IHubContext<VehicleHub> hubContext, IHubContext<ActivityLogHub> activityLogHubContext) : base(logService)
        {
            _vehicleService = vehicleService;
            _hubContext = hubContext;
            SetActivityLogHubContext(activityLogHubContext);
        }

        [BindProperty]
        public CreateVehicleViewModel Input { get; set; } = new();

        public List<SelectListItem> ModelOptions { get; set; } = new();
        public List<SelectListItem> ColorOptions { get; set; } = new();
        public List<SelectListItem> StatusOptions { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            await LoadDropdownsAsync();
            return Page();
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
                var vehicle = new Vehicle
                {
                    Model = Input.VehicleModel,
                    Version = Input.Version,
                    Color = Input.Color,
                    Config = Input.Config,
                    Price = Input.Price,
                    Status = Input.Status
                };

                var createdVehicle = await _vehicleService.CreateVehicleAsync(vehicle);

                SetSuccess($"Xe {Input.VehicleModel} {Input.Version} đã được tạo thành công!");
                await LogAsync("Create Vehicle", $"Created vehicle: {Input.VehicleModel} {Input.Version}");

                // Gửi thông báo SignalR cho tất cả client
                await _hubContext.Clients.All.SendAsync("VehicleCreated", createdVehicle.Id, createdVehicle.Model);

                return RedirectToPage("./Index");
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
                SetError("Có lỗi xảy ra khi tạo xe: " + ex.Message);
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

        private bool HasRole(int role)
        {
            return Request.Cookies["UserRole"] == role.ToString();
        }
    }
}
