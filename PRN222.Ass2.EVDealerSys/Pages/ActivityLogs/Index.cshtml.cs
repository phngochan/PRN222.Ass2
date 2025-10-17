using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using PRN222.Ass2.EVDealerSys.BLL.Interfaces;
using PRN222.Ass2.EVDealerSys.BusinessObjects.Models;

namespace PRN222.Ass2.EVDealerSys.Pages.ActivityLogs
{
    public class IndexModel : PageModel
    {
        private readonly IActivityLogService _activityLogService;
        private readonly IUserService _userService;

        public IndexModel(IActivityLogService activityLogService, IUserService userService)
        {
            _activityLogService = activityLogService;
            _userService = userService;
        }

        public List<ActivityLogViewModel> Logs { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? FilterUserId { get; set; }

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 20;
        public int TotalPages { get; set; }
        public int TotalCount { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            // Only Admin and Manager can view logs
            var userRole = Request.Cookies["UserRole"];
            if (userRole != "1" && userRole != "2")
                return RedirectToPage("/Dashboard/Index");

            try
            {
                IEnumerable<ActivityLog> logs;

                if (!string.IsNullOrWhiteSpace(SearchTerm))
                {
                    logs = await _activityLogService.SearchLogsAsync(SearchTerm);
                }
                else if (FilterUserId.HasValue)
                {
                    logs = await _activityLogService.GetLogsByUserIdAsync(FilterUserId.Value);
                }
                else
                {
                    logs = await _activityLogService.GetAllLogsAsync();
                }

                TotalCount = logs.Count();
                TotalPages = (int)Math.Ceiling((double)TotalCount / PageSize);

                Logs = logs
                    .Skip((PageNumber - 1) * PageSize)
                    .Take(PageSize)
                    .Select(l => new ActivityLogViewModel
                    {
                        Id = l.Id,
                        UserId = l.UserId,
                        UserName = l.User?.Name ?? "N/A",
                        Action = l.Action ?? "",
                        Description = l.Description,
                        CreatedAt = l.CreatedAt != null ? l.CreatedAt : DateTime.Now
                    })
                    .ToList();

                return Page();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải log: " + ex.Message;
                return Page();
            }
        }
    }

    public class ActivityLogViewModel
    {
        public int Id { get; set; }
        public int? UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
