using EVDealerSys.Models;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.SignalR;

using PRN222.Ass2.EVDealerSys.Base.BasePageModels;
using PRN222.Ass2.EVDealerSys.BLL.Interfaces;
using PRN222.Ass2.EVDealerSys.Hubs;

namespace PRN222.Ass2.EVDealerSys.Pages.VehiclesManagement
{
    [Authorize(Roles = "1,2")]
    public class IndexModel : BaseCrudPageModel
    {
        private readonly IVehicleService _vehicleService;

        public IndexModel(IVehicleService vehicleService, IActivityLogService logService, IHubContext<ActivityLogHub> activityLogHubContext) : base(logService)
        {
            _vehicleService = vehicleService;
            SetActivityLogHubContext(activityLogHubContext);
        }

        public VehiclesManagementViewModel ViewModel { get; set; } = new();
        public List<SelectListItem> StatusOptions { get; set; } = new();
        public List<SelectListItem> ColorOptions { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? SearchModel { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? SearchVersion { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? FilterStatus { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? FilterColor { get; set; }

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                await LoadListAsync();
                return Page();
            }
            catch (Exception ex)
            {
                SetError("Có lỗi xảy ra khi tải danh sách xe.");
                await LogAsync("Error", ex.Message);
                return Page();
            }
        }

        private async Task LoadListAsync()
        {
            int pageSize = 10;
            var (vehicles, totalCount) = await _vehicleService.GetVehiclesWithPaginationAsync(
                SearchModel, SearchVersion, FilterStatus, FilterColor, PageNumber, pageSize);

            var statistics = await _vehicleService.GetVehicleStatisticsAsync();

            ViewModel = new VehiclesManagementViewModel
            {
                Vehicles = vehicles.Select(v => new VehicleDto
                {
                    Id = v.Id,
                    Model = v.Model,
                    Version = v.Version,
                    Color = v.Color,
                    Config = v.Config,
                    Price = v.Price,
                    Status = v.Status
                }).ToList(),

                TotalVehicles = statistics.TotalVehicles,
                AvailableVehicles = statistics.AvailableVehicles,
                MaintenanceVehicles = statistics.MaintenanceVehicles,
                SoldVehicles = statistics.SoldVehicles,

                SearchModel = SearchModel,
                SearchVersion = SearchVersion,
                FilterStatus = FilterStatus,
                FilterColor = FilterColor,
                CurrentPage = PageNumber,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            };

            StatusOptions = GetStatusSelectList();
            ColorOptions = await GetColorSelectList();

            await LogAsync("View Vehicle List", $"SearchModel={SearchModel}, FilterStatus={FilterStatus}");
        }

        private List<SelectListItem> GetStatusSelectList()
        {
            return new List<SelectListItem>
            {
                new SelectListItem { Value = "", Text = "-- Tất cả trạng thái --" },
                new SelectListItem { Value = "1", Text = "Có sẵn" },
                new SelectListItem { Value = "2", Text = "Đã bán" },
                new SelectListItem { Value = "3", Text = "Bảo trì" },
                new SelectListItem { Value = "4", Text = "Đặt trước" }
            };
        }

        private async Task<List<SelectListItem>> GetColorSelectList()
        {
            var colors = new List<SelectListItem>
            {
                new SelectListItem { Value = "", Text = "-- Tất cả màu --" }
            };
            try
            {
                var colorList = await _vehicleService.GetDistinctColorsAsync();
                colors.AddRange(colorList.Select(c => new SelectListItem
                {
                    Value = c,
                    Text = c
                }));
            }
            catch { }
            return colors;
        }

    }
}
