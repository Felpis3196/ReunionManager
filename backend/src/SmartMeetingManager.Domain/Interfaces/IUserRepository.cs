using SmartMeetingManager.Domain.Entities;

namespace SmartMeetingManager.Domain.Interfaces;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
}