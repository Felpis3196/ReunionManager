using Microsoft.EntityFrameworkCore;
using SmartMeetingManager.Domain.Entities;
using SmartMeetingManager.Domain.Interfaces;
using SmartMeetingManager.Infrastructure.Data;
using Task = System.Threading.Tasks.Task;

namespace SmartMeetingManager.Infrastructure.Repositories;

public class IntegrationRepository : IIntegrationRepository
{
    private readonly ApplicationDbContext _context;

    public IntegrationRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Integration?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Integrations.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<Integration?> GetByOrganizationAndTypeAsync(Guid organizationId, IntegrationType type, CancellationToken cancellationToken = default)
    {
        return await _context.Integrations
            .FirstOrDefaultAsync(i => i.OrganizationId == organizationId && i.Type == type, cancellationToken);
    }

    public async Task<IEnumerable<Integration>> GetByOrganizationIdAsync(Guid organizationId, CancellationToken cancellationToken = default)
    {
        return await _context.Integrations
            .Where(i => i.OrganizationId == organizationId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Integration>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Integrations.ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Integration>> FindAsync(System.Linq.Expressions.Expression<Func<Integration, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _context.Integrations.Where(predicate).ToListAsync(cancellationToken);
    }

    public async Task<Integration> AddAsync(Integration entity, CancellationToken cancellationToken = default)
    {
        await _context.Integrations.AddAsync(entity, cancellationToken);
        return entity;
    }

    public Task UpdateAsync(Integration entity, CancellationToken cancellationToken = default)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        _context.Integrations.Update(entity);
        return System.Threading.Tasks.Task.CompletedTask;
    }

    public Task DeleteAsync(Integration entity, CancellationToken cancellationToken = default)
    {
        _context.Integrations.Remove(entity);
        return System.Threading.Tasks.Task.CompletedTask;
    }

    public async Task<bool> ExistsAsync(System.Linq.Expressions.Expression<Func<Integration, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _context.Integrations.AnyAsync(predicate, cancellationToken);
    }
}