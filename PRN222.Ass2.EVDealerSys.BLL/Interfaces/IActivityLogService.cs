namespace PRN222.Ass2.EVDealerSys.BLL.Interfaces;
public interface IActivityLogService
{
    Task LogAsync(int? userId, string action, string? description = null);
}
