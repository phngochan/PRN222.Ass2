using PRN222.Ass2.EVDealerSys.BusinessObjects.Models;

namespace PRN222.Ass2.EVDealerSys.BLL.Interfaces;
public interface IActivityLogService
{
    // userId: optional numeric id, userName: optional display name at time of action
    Task LogAsync(int? userId, string? userName, string action, string? description = null);
    Task<IEnumerable<ActivityLog>> GetAllLogsAsync();
    Task<IEnumerable<ActivityLog>> GetLogsByUserIdAsync(int userId);
    Task<IEnumerable<ActivityLog>> SearchLogsAsync(string searchTerm);
}
