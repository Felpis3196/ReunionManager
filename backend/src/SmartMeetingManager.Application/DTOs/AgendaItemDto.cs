namespace SmartMeetingManager.Application.DTOs;

public record AgendaItemDto
{
    public Guid Id { get; init; }
    public int Order { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public TimeSpan? EstimatedDuration { get; init; }
    public bool IsCompleted { get; init; }
}