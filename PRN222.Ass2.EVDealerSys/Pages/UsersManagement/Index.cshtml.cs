using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

using PRN222.Ass2.EVDealerSys.BLL.Interfaces;
using PRN222.Ass2.EVDealerSys.BusinessObjects.Models;
using PRN222.Ass2.EVDealerSys.Models;

namespace PRN222.Ass2.EVDealerSys.Pages.UsersManagement;

public class IndexModel : PageModel
{
    private readonly IUserService _userService;
    private readonly IDealerService _dealerService;

    public IndexModel(IUserService userService, IDealerService dealerService)
    {
        _userService = userService;
        _dealerService = dealerService;
    }

    public UserListViewModel ViewModel { get; set; } = new();
    public List<SelectListItem> RoleOptions { get; set; } = new();
    public List<SelectListItem> DealerOptions { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public string? SearchTerm { get; set; }
    [BindProperty(SupportsGet = true)]
    public int? FilterRole { get; set; }
    [BindProperty(SupportsGet = true)]
    public int? FilterDealer { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        IEnumerable<User> users;
        if (!string.IsNullOrWhiteSpace(SearchTerm))
            users = await _userService.SearchUsersAsync(SearchTerm);
        else if (FilterRole.HasValue)
            users = await _userService.GetUsersByRoleAsync(FilterRole.Value);
        else if (FilterDealer.HasValue)
            users = await _userService.GetUsersByDealerAsync(FilterDealer.Value);
        else
            users = await _userService.GetAllUsersAsync();

        ViewModel = new UserListViewModel
        {
            SearchTerm = SearchTerm,
            FilterRole = FilterRole,
            FilterDealer = FilterDealer,
            Users = users.Select(u => new UserItemViewModel
            {
                Id = u.Id,
                Name = u.Name ?? "",
                Email = u.Email ?? "",
                Phone = u.Phone,
                Role = u.Role ?? 0,
                RoleName = _userService.GetRoleName(u.Role),
                DealerId = u.DealerId,
                DealerName = u.Dealer?.Name
            }).ToList()
        };

        RoleOptions = GetRoleSelectList();
        DealerOptions = await GetDealerSelectList();
        return Page();
    }

    private List<SelectListItem> GetRoleSelectList()
    {
        return new List<SelectListItem>
        {
            new SelectListItem { Value = "", Text = "-- Chọn Role --" },
            new SelectListItem { Value = "1", Text = "Admin" },
            new SelectListItem { Value = "2", Text = "Manager" },
            new SelectListItem { Value = "3", Text = "Staff" }
        };
    }

    private async Task<List<SelectListItem>> GetDealerSelectList()
    {
        var dealers = new List<SelectListItem>
        {
            new SelectListItem { Value = "", Text = "-- Không có Dealer --" }
        };
        try
        {
            var dealerList = await _dealerService.GetAllDealersAsync();
            dealers.AddRange(dealerList.Select(d => new SelectListItem
            {
                Value = d.Id.ToString(),
                Text = d.Name ?? $"Dealer {d.Id}"
            }));
        }
        catch { }
        return dealers;
    }
}
