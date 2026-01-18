using SmartMeetingManager.Domain.Entities;

namespace SmartMeetingManager.Domain.Interfaces;

public interface IIntegrationRepository : IRepository<Integration>
{
    Task<Integration?> GetByOrganizationAndTypeAsync(Guid organizationId, IntegrationType type, CancellationToken cancellationToken = default);
    Task<IEnumerable<Integration>> GetByOrganizationIdAsync(Guid organizationId, CancellationToken cancellationToken = default);
}