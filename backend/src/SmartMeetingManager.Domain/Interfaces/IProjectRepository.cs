using SmartMeetingManager.Domain.Entities;

namespace SmartMeetingManager.Domain.Interfaces;

public interface IProjectRepository : IRepository<Project>
{
    Task<IEnumerable<Project>> GetByOrganizationIdAsync(Guid organizationId, CancellationToken cancellationToken = default);
}