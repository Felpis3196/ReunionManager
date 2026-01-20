using Microsoft.EntityFrameworkCore;
using SmartMeetingManager.Domain.Entities;
using SmartMeetingManager.Domain.Interfaces;
using SmartMeetingManager.Infrastructure.Data;
using Task = System.Threading.Tasks.Task;

namespace SmartMeetingManager.Infrastructure.Repositories;

public class ProjectRepository : IProjectRepository
{
    private readonly ApplicationDbContext _context;

    public ProjectRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Project?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Projects.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<IEnumerable<Project>> GetByOrganizationIdAsync(Guid organizationId, CancellationToken cancellationToken = default)
    {
        return await _context.Projects
            .Where(p => p.OrganizationId == organizationId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Project>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Projects.ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Project>> FindAsync(System.Linq.Expressions.Expression<Func<Project, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _context.Projects.Where(predicate).ToListAsync(cancellationToken);
    }

    public async Task<Project> AddAsync(Project entity, CancellationToken cancellationToken = default)
    {
        await _context.Projects.AddAsync(entity, cancellationToken);
        return entity;
    }

    public Task UpdateAsync(Project entity, CancellationToken cancellationToken = default)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        _context.Projects.Update(entity);
        return System.Threading.Tasks.Task.CompletedTask;
    }

    public Task DeleteAsync(Project entity, CancellationToken cancellationToken = default)
    {
        _context.Projects.Remove(entity);
        return System.Threading.Tasks.Task.CompletedTask;
    }

    public async Task<bool> ExistsAsync(System.Linq.Expressions.Expression<Func<Project, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _context.Projects.AnyAsync(predicate, cancellationToken);
    }
}