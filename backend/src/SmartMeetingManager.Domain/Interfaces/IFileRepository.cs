using SmartMeetingManager.Domain.Entities;
using TaskEntity = SmartMeetingManager.Domain.Entities.Task;

namespace SmartMeetingManager.Domain.Interfaces;

public interface IFileRepository
{
    System.Threading.Tasks.Task<MeetingFile?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    System.Threading.Tasks.Task<IEnumerable<MeetingFile>> GetByMeetingIdAsync(Guid meetingId, CancellationToken cancellationToken = default);
    System.Threading.Tasks.Task<MeetingFile> AddAsync(MeetingFile file, CancellationToken cancellationToken = default);
    System.Threading.Tasks.Task DeleteAsync(MeetingFile file, CancellationToken cancellationToken = default);
}
