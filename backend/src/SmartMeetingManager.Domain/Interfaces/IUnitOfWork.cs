namespace SmartMeetingManager.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IMeetingRepository Meetings { get; }
    IUserRepository Users { get; }
    IOrganizationRepository Organizations { get; }
    IProjectRepository Projects { get; }
    ITaskRepository Tasks { get; }
    ITranscriptRepository Transcripts { get; }
    IIntegrationRepository Integrations { get; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}