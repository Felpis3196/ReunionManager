using SmartMeetingManager.Application.DTOs;
using SmartMeetingManager.Application.Interfaces;
using SmartMeetingManager.Domain.Entities;
using SmartMeetingManager.Domain.Interfaces;

namespace SmartMeetingManager.Infrastructure.Services;

public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;

    public UserService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<UserDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id, cancellationToken);
        return user == null ? null : MapToDto(user);
    }

    public async Task<UserDto?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email))
            return null;
        var user = await _unitOfWork.Users.GetByEmailAsync(email.Trim(), cancellationToken);
        return user == null ? null : MapToDto(user);
    }

    public async Task<IEnumerable<UserDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var users = await _unitOfWork.Users.GetAllAsync(cancellationToken);
        return users.Select(MapToDto);
    }

    public async Task<IEnumerable<UserDto>> SearchAsync(string searchTerm, int take = 10, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return Array.Empty<UserDto>();
        var term = searchTerm.Trim().ToLowerInvariant();
        var users = await _unitOfWork.Users.FindAsync(u =>
            u.Name.ToLower().Contains(term) || u.Email.ToLower().Contains(term), cancellationToken);
        return users.Take(take).Select(MapToDto);
    }

    public async Task<UserDto?> CreateAsync(CreateUserDto dto, CancellationToken cancellationToken = default)
    {
        var email = dto.Email.Trim();
        var existing = await _unitOfWork.Users.GetByEmailAsync(email, cancellationToken);
        if (existing != null)
            return null;

        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = dto.Name.Trim(),
            Email = email,
            PasswordHash = HashPassword(dto.Password),
            IsActive = true,
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow
        };
        await _unitOfWork.Users.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return MapToDto(user);
    }

    public async Task<UserDto?> UpdateAsync(Guid id, UpdateUserDto dto, CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id, cancellationToken);
        if (user == null)
            return null;

        if (dto.Name != null)
            user.Name = dto.Name.Trim();
        if (dto.AvatarUrl != null)
            user.AvatarUrl = dto.AvatarUrl.Trim();

        user.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.Users.UpdateAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return MapToDto(user);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id, cancellationToken);
        if (user == null)
            return false;

        await _unitOfWork.Users.DeleteAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static UserDto MapToDto(User u)
    {
        return new UserDto
        {
            Id = u.Id,
            Name = u.Name,
            Email = u.Email,
            AvatarUrl = u.AvatarUrl,
            IsActive = u.IsActive,
            CreatedAt = u.CreatedAt
        };
    }

    private static string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
    }
}
