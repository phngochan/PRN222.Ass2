using PRN222.Ass2.EVDealerSys.BLL.Interfaces;
using PRN222.Ass2.EVDealerSys.BusinessObjects.Models;
using PRN222.Ass2.EVDealerSys.DAL.Interfaces;

namespace PRN222.Ass2.EVDealerSys.BLL.Implementations;
public class ActivityLogService : IActivityLogService
{
    private readonly IActivityLogRepository _activityLogRepository;

    public ActivityLogService(IActivityLogRepository activityLogRepository)
    {
        _activityLogRepository = activityLogRepository;
    }

    public async Task<ActivityLog> LogAsync(int? userId, string? userName, string action, string? description = null)
    {
        var log = new ActivityLog
        {
            UserId = userId,
            Action = action,
            Description = description,
            CreatedAt = DateTime.Now
        };

        _activityLogRepository.Create(log);
        await _activityLogRepository.SaveAsync();
        return log;
    }

    public async Task<IEnumerable<ActivityLog>> GetAllLogsAsync()
    {
        var logs = await _activityLogRepository.GetAllAsync(log => log.User);
        return logs.OrderByDescending(l => l.CreatedAt);
    }

    public async Task<IEnumerable<ActivityLog>> GetLogsByUserIdAsync(int userId)
    {
        var logs = await _activityLogRepository.GetAllAsync(log => log.User);
        return logs.Where(l => l.UserId == userId).OrderByDescending(l => l.CreatedAt);
    }

    public async Task<IEnumerable<ActivityLog>> SearchLogsAsync(string searchTerm)
    {
        var logs = await _activityLogRepository.GetAllAsync(log => log.User);
        return logs.Where(l =>
            (l.Action != null && l.Action.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) ||
            (l.Description != null && l.Description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) ||
            (l.User != null && l.User.Name != null && l.User.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) ||
            (l.User.Name != null && l.User.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
        ).OrderByDescending(l => l.CreatedAt);
    }
}
