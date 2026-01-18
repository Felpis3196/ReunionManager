using Microsoft.EntityFrameworkCore;
using SmartMeetingManager.Domain.Entities;
using SmartMeetingManager.Domain.Interfaces;
using SmartMeetingManager.Infrastructure.Data;

namespace SmartMeetingManager.Infrastructure.Repositories;

public class TaskRepository : ITaskRepository
{
    private readonly ApplicationDbContext _context;

    public TaskRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Task?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Tasks.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<IEnumerable<Task>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Tasks
            .Where(t => t.AssignedToId == userId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Task>> GetByMeetingIdAsync(Guid meetingId, CancellationToken cancellationToken = default)
    {
        return await _context.Tasks
            .Where(t => t.MeetingId == meetingId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Task>> GetPendingByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Tasks
            .Where(t => t.AssignedToId == userId && t.Status == Domain.Entities.TaskStatus.Pending)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Task>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Tasks.ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Task>> FindAsync(System.Linq.Expressions.Expression<Func<Task, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _context.Tasks.Where(predicate).ToListAsync(cancellationToken);
    }

    public async Task<Task> AddAsync(Task entity, CancellationToken cancellationToken = default)
    {
        await _context.Tasks.AddAsync(entity, cancellationToken);
        return entity;
    }

    public Task UpdateAsync(Task entity, CancellationToken cancellationToken = default)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        _context.Tasks.Update(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Task entity, CancellationToken cancellationToken = default)
    {
        _context.Tasks.Remove(entity);
        return Task.CompletedTask;
    }

    public async Task<bool> ExistsAsync(System.Linq.Expressions.Expression<Func<Task, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _context.Tasks.AnyAsync(predicate, cancellationToken);
    }
}