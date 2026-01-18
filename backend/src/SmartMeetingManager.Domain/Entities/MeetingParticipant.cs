namespace SmartMeetingManager.Domain.Entities;

public enum ParticipantStatus
{
    Invited,
    Accepted,
    Declined,
    Attended,
    Absent
}

public class MeetingParticipant
{
    public Guid Id { get; set; }
    public Guid MeetingId { get; set; }
    public Guid UserId { get; set; }
    public ParticipantStatus Status { get; set; }
    public DateTime? InvitedAt { get; set; }
    public DateTime? RespondedAt { get; set; }
    public DateTime? AttendedAt { get; set; }
    
    // Navigation properties
    public virtual Meeting Meeting { get; set; } = null!;
    public virtual User User { get; set; } = null!;
}