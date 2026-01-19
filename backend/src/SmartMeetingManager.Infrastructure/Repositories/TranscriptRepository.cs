using Microsoft.EntityFrameworkCore;
using SmartMeetingManager.Domain.Entities;
using SmartMeetingManager.Domain.Interfaces;
using SmartMeetingManager.Infrastructure.Data;

namespace SmartMeetingManager.Infrastructure.Repositories;

public class TranscriptRepository : ITranscriptRepository
{
    private readonly ApplicationDbContext _context;

    public TranscriptRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Transcript?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Transcripts.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<IEnumerable<Transcript>> GetByMeetingIdAsync(Guid meetingId, CancellationToken cancellationToken = default)
    {
        return await _context.Transcripts
            .Where(t => t.MeetingId == meetingId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Transcript>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Transcripts.ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Transcript>> FindAsync(System.Linq.Expressions.Expression<Func<Transcript, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _context.Transcripts.Where(predicate).ToListAsync(cancellationToken);
    }

    public async Task<Transcript> AddAsync(Transcript entity, CancellationToken cancellationToken = default)
    {
        await _context.Transcripts.AddAsync(entity, cancellationToken);
        return entity;
    }

    public System.Threading.Tasks.Task UpdateAsync(Transcript entity, CancellationToken cancellationToken = default)
    {
        _context.Transcripts.Update(entity);
        return System.Threading.Tasks.Task.CompletedTask;
    }

    public System.Threading.Tasks.Task DeleteAsync(Transcript entity, CancellationToken cancellationToken = default)
    {
        _context.Transcripts.Remove(entity);
        return System.Threading.Tasks.Task.CompletedTask;
    }

    public async Task<bool> ExistsAsync(System.Linq.Expressions.Expression<Func<Transcript, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _context.Transcripts.AnyAsync(predicate, cancellationToken);
    }
}
