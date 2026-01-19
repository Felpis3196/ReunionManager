using SmartMeetingManager.Domain.Entities;

namespace SmartMeetingManager.Domain.Interfaces;

public interface ITranscriptRepository : IRepository<Transcript>
{
    Task<IEnumerable<Transcript>> GetByMeetingIdAsync(Guid meetingId, CancellationToken cancellationToken = default);
}
