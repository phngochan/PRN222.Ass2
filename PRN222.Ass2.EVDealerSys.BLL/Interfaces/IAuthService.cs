using PRN222.Ass2.EVDealerSys.BusinessObjects.Models;

namespace PRN222.Ass2.EVDealerSys.BLL.Interfaces;
public interface IAuthService
{
    Task<User?> AuthenticateAsync(string email, string password);
    bool IsValidRole(int? role);
}
