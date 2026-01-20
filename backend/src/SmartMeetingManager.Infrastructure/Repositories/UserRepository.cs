using Microsoft.EntityFrameworkCore;
using SmartMeetingManager.Domain.Entities;
using SmartMeetingManager.Domain.Interfaces;
using SmartMeetingManager.Infrastructure.Data;
using Task = System.Threading.Tasks.Task;

namespace SmartMeetingManager.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;

    public UserRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Users.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    public async Task<IEnumerable<User>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Users.ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<User>> FindAsync(System.Linq.Expressions.Expression<Func<User, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _context.Users.Where(predicate).ToListAsync(cancellationToken);
    }

    public async Task<User> AddAsync(User entity, CancellationToken cancellationToken = default)
    {
        await _context.Users.AddAsync(entity, cancellationToken);
        return entity;
    }

    public Task UpdateAsync(User entity, CancellationToken cancellationToken = default)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        _context.Users.Update(entity);
        return System.Threading.Tasks.Task.CompletedTask;
    }

    public Task DeleteAsync(User entity, CancellationToken cancellationToken = default)
    {
        _context.Users.Remove(entity);
        return System.Threading.Tasks.Task.CompletedTask;
    }

    public async Task<bool> ExistsAsync(System.Linq.Expressions.Expression<Func<User, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _context.Users.AnyAsync(predicate, cancellationToken);
    }
}