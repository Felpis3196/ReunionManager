using Microsoft.EntityFrameworkCore;
using SmartMeetingManager.Domain.Entities;
using SmartMeetingManager.Domain.Interfaces;
using SmartMeetingManager.Infrastructure.Data;
using Task = System.Threading.Tasks.Task;

namespace SmartMeetingManager.Infrastructure.Repositories;

public class MeetingRepository : IMeetingRepository
{
    private readonly ApplicationDbContext _context;

    public MeetingRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Meeting?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Meetings.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<Meeting?> GetWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Meetings
            .Include(m => m.Organization)
            .Include(m => m.Project)
            .Include(m => m.Organizer)
            .Include(m => m.Participants).ThenInclude(p => p.User)
            .Include(m => m.AgendaItems)
            .Include(m => m.Decisions).ThenInclude(d => d.MadeBy)
            .Include(m => m.Tasks).ThenInclude(t => t.AssignedTo)
            .Include(m => m.Transcripts)
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Meeting>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Meetings.ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Meeting>> GetByOrganizationIdAsync(Guid organizationId, CancellationToken cancellationToken = default)
    {
        return await _context.Meetings
            .Where(m => m.OrganizationId == organizationId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Meeting>> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        return await _context.Meetings
            .Where(m => m.ProjectId == projectId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Meeting>> GetByOrganizerIdAsync(Guid organizerId, CancellationToken cancellationToken = default)
    {
        return await _context.Meetings
            .Where(m => m.OrganizerId == organizerId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Meeting>> GetUpcomingByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        return await _context.Meetings
            .Where(m => m.ScheduledAt > now && 
                       (m.OrganizerId == userId || m.Participants.Any(p => p.UserId == userId)))
            .OrderBy(m => m.ScheduledAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Meeting>> FindAsync(System.Linq.Expressions.Expression<Func<Meeting, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _context.Meetings.Where(predicate).ToListAsync(cancellationToken);
    }

    public async Task<Meeting> AddAsync(Meeting entity, CancellationToken cancellationToken = default)
    {
        await _context.Meetings.AddAsync(entity, cancellationToken);
        return entity;
    }

    public Task UpdateAsync(Meeting entity, CancellationToken cancellationToken = default)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        _context.Meetings.Update(entity);
        return System.Threading.Tasks.Task.CompletedTask;
    }

    public Task DeleteAsync(Meeting entity, CancellationToken cancellationToken = default)
    {
        _context.Meetings.Remove(entity);
        return System.Threading.Tasks.Task.CompletedTask;
    }

    public async Task<bool> ExistsAsync(System.Linq.Expressions.Expression<Func<Meeting, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _context.Meetings.AnyAsync(predicate, cancellationToken);
    }
}