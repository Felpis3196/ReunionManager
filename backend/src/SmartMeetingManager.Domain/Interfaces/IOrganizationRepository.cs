using SmartMeetingManager.Domain.Entities;

namespace SmartMeetingManager.Domain.Interfaces;

public interface IOrganizationRepository : IRepository<Organization>
{
    Task<IEnumerable<Organization>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
}