using SmartMeetingManager.Domain.Entities;
using TaskEntity = SmartMeetingManager.Domain.Entities.Task;
using Task = System.Threading.Tasks.Task;

namespace SmartMeetingManager.Domain.Interfaces;

public interface ITaskRepository : IRepository<TaskEntity>
{
    Task<IEnumerable<TaskEntity>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<TaskEntity>> GetByMeetingIdAsync(Guid meetingId, CancellationToken cancellationToken = default);
    Task<IEnumerable<TaskEntity>> GetPendingByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
}