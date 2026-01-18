using SmartMeetingManager.Domain.Entities;

namespace SmartMeetingManager.Application.DTOs;

public record TaskDto
{
    public Guid Id { get; init; }
    public Guid MeetingId { get; init; }
    public Guid? ProjectId { get; init; }
    public Guid AssignedToId { get; init; }
    public string AssignedToName { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public TaskStatus Status { get; init; }
    public TaskPriority Priority { get; init; }
    public DateTime? DueDate { get; init; }
    public DateTime? CompletedAt { get; init; }
}