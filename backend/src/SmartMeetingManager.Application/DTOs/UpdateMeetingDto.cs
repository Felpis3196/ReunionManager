using SmartMeetingManager.Domain.Entities;

namespace SmartMeetingManager.Application.DTOs;

public record UpdateMeetingDto
{
    public string? Title { get; init; }
    public string? Description { get; init; }
    public MeetingType? Type { get; init; }
    public DateTime? ScheduledAt { get; init; }
    public TimeSpan? Duration { get; init; }
    public string? Location { get; init; }
    public string? MeetingUrl { get; init; }
    public string? FinalAgenda { get; init; }
}