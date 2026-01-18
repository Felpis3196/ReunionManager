using SmartMeetingManager.Domain.Entities;

namespace SmartMeetingManager.Application.DTOs;

public record MeetingDto
{
    public Guid Id { get; init; }
    public Guid OrganizationId { get; init; }
    public Guid? ProjectId { get; init; }
    public Guid OrganizerId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public MeetingType Type { get; init; }
    public MeetingStatus Status { get; init; }
    public DateTime ScheduledAt { get; init; }
    public TimeSpan Duration { get; init; }
    public string? Location { get; init; }
    public string? MeetingUrl { get; init; }
    public string? SuggestedAgenda { get; init; }
    public string? FinalAgenda { get; init; }
    public string? Summary { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? StartedAt { get; init; }
    public DateTime? EndedAt { get; init; }
    public string? ProjectName { get; init; }
    public string OrganizerName { get; init; } = string.Empty;
    public IEnumerable<ParticipantDto> Participants { get; init; } = Array.Empty<ParticipantDto>();
    public IEnumerable<AgendaItemDto> AgendaItems { get; init; } = Array.Empty<AgendaItemDto>();
    public IEnumerable<DecisionDto> Decisions { get; init; } = Array.Empty<DecisionDto>();
    public IEnumerable<TaskDto> Tasks { get; init; } = Array.Empty<TaskDto>();
}