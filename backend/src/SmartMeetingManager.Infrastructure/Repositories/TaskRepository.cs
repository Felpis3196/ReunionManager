using Microsoft.EntityFrameworkCore;
using SmartMeetingManager.Domain.Entities;
using SmartMeetingManager.Domain.Interfaces;
using SmartMeetingManager.Infrastructure.Data;
using TaskEntity = SmartMeetingManager.Domain.Entities.Task;

namespace SmartMeetingManager.Infrastructure.Repositories;

public class TaskRepository : ITaskRepository
{
    private readonly ApplicationDbContext _context;

    public TaskRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<TaskEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Tasks.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<IEnumerable<TaskEntity>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Tasks
            .Where(t => t.AssignedToId == userId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<TaskEntity>> GetByMeetingIdAsync(Guid meetingId, CancellationToken cancellationToken = default)
    {
        return await _context.Tasks
            .Where(t => t.MeetingId == meetingId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<TaskEntity>> GetPendingByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Tasks
            .Where(t => t.AssignedToId == userId && t.Status == Domain.Entities.TaskStatus.Pending)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<TaskEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Tasks.ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<TaskEntity>> FindAsync(System.Linq.Expressions.Expression<Func<TaskEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _context.Tasks.Where(predicate).ToListAsync(cancellationToken);
    }

    public async Task<TaskEntity> AddAsync(TaskEntity entity, CancellationToken cancellationToken = default)
    {
        await _context.Tasks.AddAsync(entity, cancellationToken);
        return entity;
    }

    public System.Threading.Tasks.Task UpdateAsync(TaskEntity entity, CancellationToken cancellationToken = default)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        _context.Tasks.Update(entity);
        return System.Threading.Tasks.Task.CompletedTask;
    }

    public System.Threading.Tasks.Task DeleteAsync(TaskEntity entity, CancellationToken cancellationToken = default)
    {
        _context.Tasks.Remove(entity);
        return System.Threading.Tasks.Task.CompletedTask;
    }

    public async Task<bool> ExistsAsync(System.Linq.Expressions.Expression<Func<TaskEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _context.Tasks.AnyAsync(predicate, cancellationToken);
    }
}