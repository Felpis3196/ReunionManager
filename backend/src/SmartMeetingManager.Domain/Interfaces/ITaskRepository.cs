using SmartMeetingManager.Domain.Entities;
using TaskEntity = SmartMeetingManager.Domain.Entities.Task;
using TaskStatus = SmartMeetingManager.Domain.Entities.TaskStatus;
using Task = System.Threading.Tasks.Task;

namespace SmartMeetingManager.Domain.Interfaces;

public interface ITaskRepository : IRepository<TaskEntity>
{
    Task<IEnumerable<TaskEntity>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<TaskEntity>> GetByMeetingIdAsync(Guid meetingId, CancellationToken cancellationToken = default);
    Task<IEnumerable<TaskEntity>> GetPendingByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<TaskEntity>> GetByOrganizationIdAsync(Guid organizationId, Guid? assignedToId, TaskStatus? status, CancellationToken cancellationToken = default);
    Task<TaskEntity?> GetByIdWithMeetingAsync(Guid id, CancellationToken cancellationToken = default);
}