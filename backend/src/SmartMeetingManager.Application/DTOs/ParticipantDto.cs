using SmartMeetingManager.Domain.Entities;

namespace SmartMeetingManager.Application.DTOs;

public record ParticipantDto
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public string UserName { get; init; } = string.Empty;
    public string UserEmail { get; init; } = string.Empty;
    public ParticipantStatus Status { get; init; }
    public DateTime? InvitedAt { get; init; }
    public DateTime? AttendedAt { get; init; }
}