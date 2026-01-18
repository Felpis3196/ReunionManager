using SmartMeetingManager.Domain.Entities;

namespace SmartMeetingManager.Domain.Interfaces;

public interface ITaskRepository : IRepository<Task>
{
    Task<IEnumerable<Task>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Task>> GetByMeetingIdAsync(Guid meetingId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Task>> GetPendingByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
}