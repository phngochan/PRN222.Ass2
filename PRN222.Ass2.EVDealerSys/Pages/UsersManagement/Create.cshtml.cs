using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

using PRN222.Ass2.EVDealerSys.BLL.Interfaces;
using PRN222.Ass2.EVDealerSys.BusinessObjects.Models;
using PRN222.Ass2.EVDealerSys.Models;

namespace PRN222.Ass2.EVDealerSys.Pages.UsersManagement;

public class CreateModel : PageModel
{
    private readonly IUserService _userService;
    private readonly IDealerService _dealerService;

    public CreateModel(IUserService userService, IDealerService dealerService)
    {
        _userService = userService;
        _dealerService = dealerService;
    }

    [BindProperty]
    public UserViewModel ViewModel { get; set; } = new();
    public List<SelectListItem> RoleOptions { get; set; } = new();
    public List<SelectListItem> DealerOptions { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        RoleOptions = GetRoleSelectList();
        DealerOptions = await GetDealerSelectList();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            RoleOptions = GetRoleSelectList();
            DealerOptions = await GetDealerSelectList();
            return Page();
        }
        var user = new User
        {
            Name = ViewModel.Name,
            Email = ViewModel.Email,
            Phone = ViewModel.Phone,
            Role = ViewModel.Role,
            DealerId = ViewModel.DealerId,
            Password = ViewModel.Password
        };
        var result = await _userService.CreateUserAsync(user);
        if (result.Success)
        {
            TempData["SuccessMessage"] = result.Message;
            return RedirectToPage("./Index");
        }
        else
        {
            ModelState.AddModelError("", result.Message);
            RoleOptions = GetRoleSelectList();
            DealerOptions = await GetDealerSelectList();
            return Page();
        }
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
