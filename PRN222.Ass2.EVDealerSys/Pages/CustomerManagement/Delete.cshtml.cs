using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

using PRN222.Ass2.EVDealerSys.Base.BasePageModels;
using PRN222.Ass2.EVDealerSys.BLL.Interfaces;
using PRN222.Ass2.EVDealerSys.Models.CustomerManagement;
using PRN222.Ass2.EVDealerSys.Hubs;

namespace PRN222.Ass2.EVDealerSys.Pages.CustomerManagement
{
    [Authorize(Roles = "1,2")]
    public class DeleteModel : BaseCrudPageModel
    {
        private readonly ICustomerService _customerService;
        private readonly IHubContext<ManagementHub> _hubContext;

        public DeleteModel(IActivityLogService logService, ICustomerService customerService, IHubContext<ManagementHub> hubContext, IHubContext<ActivityLogHub> activityLogHubContext) : base(logService)
        {
            _customerService = customerService;
            _hubContext = hubContext;
            SetActivityLogHubContext(activityLogHubContext);
        }

        public DeleteCustomerViewModel ViewModel { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            try
            {
                var customer = await _customerService.GetCustomerByIdAsync(id);
                if (customer == null)
                {
                    SetError("Không tìm thấy khách hàng.");
                    return RedirectToAction(nameof(Index));
                }

                var viewModel = new DeleteCustomerViewModel
                {
                    Id = customer.Id,
                    Name = customer.Name ?? "",
                    Phone = customer.Phone ?? "",
                    Email = customer.Email ?? "",
                    Address = customer.Address ?? "",
                    DealerName = customer.Dealer?.Name ?? "",
                    OrderCount = customer.Orders?.Count ?? 0
                };

                await LogAsync("View Deleted Customer", $"ID={id}");
                return Page();
            }
            catch (Exception ex)
            {
                SetError("Đã xảy ra lỗi khi tải dữ liệu.");
                await LogAsync("Error", ex.Message);
                return RedirectToAction(nameof(Index));
            }
        }
        public async Task<IActionResult> OnPostAsync(int id)
        {
            try
            {
                var customerName = ViewModel.Name; // Store name before delete
                var success = await _customerService.DeleteCustomerAsync(id);
                if (success)
                {
                    SetSuccess("Xóa khách hàng thành công!");
                    await LogAsync("Delete Customer", $"Deleted: {customerName} (ID={id})");
                    
                    // Send SignalR notification
                    await _hubContext.Clients.All.SendAsync("ReceiveCustomerDeleted", new
                    {
                        id = id,
                        name = customerName
                    });
                }
                else
                {
                    SetError("Không thể xóa khách hàng.");
                }
                return RedirectToPage("./Index");
            }
            catch (Exception)
            {
                SetError("Có lỗi xảy ra khi cập nhật thông tin khách hàng.");
                return Page();
            }
        }
    }
}
