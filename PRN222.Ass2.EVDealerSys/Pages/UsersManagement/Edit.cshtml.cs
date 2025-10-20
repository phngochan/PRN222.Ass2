using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

using PRN222.Ass2.EVDealerSys.BLL.Interfaces;
using PRN222.Ass2.EVDealerSys.Models;

namespace PRN222.Ass2.EVDealerSys.Pages.UsersManagement;

[Authorize(Roles = "1,2")]
public class EditModel : PageModel
{
    private readonly IUserService _userService;
    private readonly IDealerService _dealerService;

    public EditModel(IUserService userService, IDealerService dealerService)
    {
        _userService = userService;
        _dealerService = dealerService;
    }

    [BindProperty]
    public UserEditViewModel ViewModel { get; set; } = new();
    public List<SelectListItem> RoleOptions { get; set; } = new();
    public List<SelectListItem> DealerOptions { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var user = await _userService.GetUserByIdAsync(id);
        if (user == null)
            return NotFound();
        ViewModel = new UserEditViewModel
        {
            Id = user.Id,
            Name = user.Name ?? "",
            Email = user.Email ?? "",
            Phone = user.Phone,
            Role = user.Role ?? 0,
            DealerId = user.DealerId,
            DealerName = user.Dealer?.Name,
            RoleName = _userService.GetRoleName(user.Role)
        };

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
        if (ViewModel.Role != 3)
        {
            return Forbid();
        }

        // Get existing user first
        var existingUser = await _userService.GetUserByIdAsync(ViewModel.Id);
        if (existingUser == null)
        {
            ModelState.AddModelError("", "User không tồn tại!");
            RoleOptions = GetRoleSelectList();
            DealerOptions = await GetDealerSelectList();
            return Page();
        }

        // Update properties
        existingUser.Name = ViewModel.Name;
        existingUser.Email = ViewModel.Email;
        existingUser.Phone = ViewModel.Phone;
        existingUser.Role = ViewModel.Role;
        existingUser.DealerId = ViewModel.DealerId;
        existingUser.Password = string.IsNullOrWhiteSpace(ViewModel.Password) ? null : ViewModel.Password;

        var result = await _userService.UpdateUserAsync(existingUser);
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
