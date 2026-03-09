using Microsoft.EntityFrameworkCore;
using SmartMeetingManager.Domain.Entities;
using SmartMeetingManager.Domain.Interfaces;
using SmartMeetingManager.Infrastructure.Data;
using TaskEntity = SmartMeetingManager.Domain.Entities.Task;

namespace SmartMeetingManager.Infrastructure.Repositories;

public class FileRepository : IFileRepository
{
    private readonly ApplicationDbContext _context;

    public FileRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async System.Threading.Tasks.Task<MeetingFile?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.MeetingFiles
            .Include(f => f.UploadedBy)
            .FirstOrDefaultAsync(f => f.Id == id, cancellationToken);
    }

    public async System.Threading.Tasks.Task<IEnumerable<MeetingFile>> GetByMeetingIdAsync(Guid meetingId, CancellationToken cancellationToken = default)
    {
        return await _context.MeetingFiles
            .Include(f => f.UploadedBy)
            .Where(f => f.MeetingId == meetingId)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async System.Threading.Tasks.Task<MeetingFile> AddAsync(MeetingFile file, CancellationToken cancellationToken = default)
    {
        await _context.MeetingFiles.AddAsync(file, cancellationToken);
        return file;
    }

    public System.Threading.Tasks.Task DeleteAsync(MeetingFile file, CancellationToken cancellationToken = default)
    {
        _context.MeetingFiles.Remove(file);
        return System.Threading.Tasks.Task.CompletedTask;
    }
}
