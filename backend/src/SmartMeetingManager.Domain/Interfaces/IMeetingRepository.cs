using SmartMeetingManager.Domain.Entities;

namespace SmartMeetingManager.Domain.Interfaces;

public interface IMeetingRepository : IRepository<Meeting>
{
    Task<IEnumerable<Meeting>> GetByOrganizationIdAsync(Guid organizationId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Meeting>> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Meeting>> GetByOrganizerIdAsync(Guid organizerId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Meeting>> GetUpcomingByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Meeting?> GetWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
}