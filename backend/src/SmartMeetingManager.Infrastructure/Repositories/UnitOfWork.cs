using SmartMeetingManager.Domain.Interfaces;
using SmartMeetingManager.Infrastructure.Data;

namespace SmartMeetingManager.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private IMeetingRepository? _meetings;
    private IUserRepository? _users;
    private IOrganizationRepository? _organizations;
    private IProjectRepository? _projects;
    private ITaskRepository? _tasks;
    private ITranscriptRepository? _transcripts;
    private IIntegrationRepository? _integrations;
    private Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction? _transaction;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
    }

    public IMeetingRepository Meetings => _meetings ??= new MeetingRepository(_context);
    public IUserRepository Users => _users ??= new UserRepository(_context);
    public IOrganizationRepository Organizations => _organizations ??= new OrganizationRepository(_context);
    public IProjectRepository Projects => _projects ??= new ProjectRepository(_context);
    public ITaskRepository Tasks => _tasks ??= new TaskRepository(_context);
    public ITranscriptRepository Transcripts => _transcripts ??= new TranscriptRepository(_context);
    public IIntegrationRepository Integrations => _integrations ??= new IntegrationRepository(_context);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}