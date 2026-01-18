using SmartMeetingManager.Domain.Entities;

namespace SmartMeetingManager.Application.DTOs;

public record CreateMeetingDto
{
    public Guid OrganizationId { get; init; }
    public Guid? ProjectId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public MeetingType Type { get; init; }
    public DateTime ScheduledAt { get; init; }
    public TimeSpan Duration { get; init; }
    public string? Location { get; init; }
    public string? MeetingUrl { get; init; }
    public IEnumerable<Guid> ParticipantIds { get; init; } = Array.Empty<Guid>();
}