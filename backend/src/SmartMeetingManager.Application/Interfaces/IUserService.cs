using SmartMeetingManager.Application.DTOs;

namespace SmartMeetingManager.Application.Interfaces;

public interface IUserService
{
    Task<UserDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<UserDto?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<IEnumerable<UserDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<UserDto>> SearchAsync(string searchTerm, int take = 10, CancellationToken cancellationToken = default);
    Task<UserDto?> CreateAsync(CreateUserDto dto, CancellationToken cancellationToken = default);
    Task<UserDto?> UpdateAsync(Guid id, UpdateUserDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
