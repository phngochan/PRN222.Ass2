using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using PRN222.Ass2.EVDealerSys.BLL.Interfaces;
using PRN222.Ass2.EVDealerSys.Models;

namespace PRN222.Ass2.EVDealerSys.Pages.UsersManagement;

[Authorize(Roles = "1")]
public class DeleteModel : PageModel
{
    private readonly IUserService _userService;

    public DeleteModel(IUserService userService)
    {
        _userService = userService;
    }

    [BindProperty]
    public UserItemViewModel ViewModel { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        if (!HasRole(1))
            return Forbid();
        var user = await _userService.GetUserByIdAsync(id);
        if (user == null)
            return NotFound();
        ViewModel = new UserItemViewModel
        {
            Id = user.Id,
            Name = user.Name ?? "",
            Email = user.Email ?? "",
            Phone = user.Phone,
            Role = user.Role ?? 0,
            RoleName = _userService.GetRoleName(user.Role),
            DealerId = user.DealerId,
            DealerName = user.Dealer?.Name
        };
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!HasRole(1))
            return Forbid();
        var result = await _userService.DeleteUserAsync(ViewModel.Id);
        if (result.Success)
        {
            TempData["SuccessMessage"] = result.Message;
        }
        else
        {
            TempData["ErrorMessage"] = result.Message;
        }
        return RedirectToPage("./Index");
    }
    private bool HasRole(int role)
    {
        // Implement your role check logic here
        return true;
    }
}
