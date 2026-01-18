using Microsoft.EntityFrameworkCore;
using SmartMeetingManager.Domain.Entities;
using SmartMeetingManager.Domain.Interfaces;
using SmartMeetingManager.Infrastructure.Data;

namespace SmartMeetingManager.Infrastructure.Repositories;

public class OrganizationRepository : IOrganizationRepository
{
    private readonly ApplicationDbContext _context;

    public OrganizationRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Organization?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Organizations.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<IEnumerable<Organization>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Organizations
            .Where(o => o.Members.Any(m => m.UserId == userId))
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Organization>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Organizations.ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Organization>> FindAsync(System.Linq.Expressions.Expression<Func<Organization, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _context.Organizations.Where(predicate).ToListAsync(cancellationToken);
    }

    public async Task<Organization> AddAsync(Organization entity, CancellationToken cancellationToken = default)
    {
        await _context.Organizations.AddAsync(entity, cancellationToken);
        return entity;
    }

    public Task UpdateAsync(Organization entity, CancellationToken cancellationToken = default)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        _context.Organizations.Update(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Organization entity, CancellationToken cancellationToken = default)
    {
        _context.Organizations.Remove(entity);
        return Task.CompletedTask;
    }

    public async Task<bool> ExistsAsync(System.Linq.Expressions.Expression<Func<Organization, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _context.Organizations.AnyAsync(predicate, cancellationToken);
    }
}