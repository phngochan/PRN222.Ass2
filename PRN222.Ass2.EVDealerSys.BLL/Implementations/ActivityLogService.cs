using PRN222.Ass2.EVDealerSys.BLL.Interfaces;
using PRN222.Ass2.EVDealerSys.BusinessObjects.Models;
using PRN222.Ass2.EVDealerSys.DAL.Interfaces;

namespace PRN222.Ass2.EVDealerSys.BLL.Implementations;
public class ActivityLogService(IActivityLogRepository activityLogRepository) : IActivityLogService
{
    private readonly IActivityLogRepository _activityLogRepository = activityLogRepository;

    public async Task LogAsync(int? userId, string action, string? description = null)
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
    }
}
