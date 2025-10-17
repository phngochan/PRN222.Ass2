using PRN222.Ass2.EVDealerSys.BusinessObjects.Models;

namespace PRN222.Ass2.EVDealerSys.BLL.Interfaces;
public interface IActivityLogService
{
    Task LogAsync(int? userId, string action, string? description = null);
    Task<IEnumerable<ActivityLog>> GetAllLogsAsync();
    Task<IEnumerable<ActivityLog>> GetLogsByUserIdAsync(int userId);
    Task<IEnumerable<ActivityLog>> SearchLogsAsync(string searchTerm);
}
