namespace SmartMeetingManager.Domain.Entities;

public enum MeetingType
{
    Planning,
    Review,
    Standup,
    Retrospective,
    OneOnOne,
    Other
}

public enum MeetingStatus
{
    Scheduled,
    InProgress,
    Completed,
    Cancelled
}

public class Meeting
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public Guid? ProjectId { get; set; }
    public Guid OrganizerId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public MeetingType Type { get; set; }
    public MeetingStatus Status { get; set; }
    public DateTime ScheduledAt { get; set; }
    public TimeSpan Duration { get; set; }
    public string? Location { get; set; }
    public string? MeetingUrl { get; set; }
    public string? SuggestedAgenda { get; set; } // IA-generated
    public string? FinalAgenda { get; set; }
    public string? Summary { get; set; } // IA-generated post-meeting
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    
    // Navigation properties
    public virtual Organization Organization { get; set; } = null!;
    public virtual Project? Project { get; set; }
    public virtual User Organizer { get; set; } = null!;
    public virtual ICollection<MeetingParticipant> Participants { get; set; } = new List<MeetingParticipant>();
    public virtual ICollection<AgendaItem> AgendaItems { get; set; } = new List<AgendaItem>();
    public virtual ICollection<Decision> Decisions { get; set; } = new List<Decision>();
    public virtual ICollection<Task> Tasks { get; set; } = new List<Task>();
    public virtual ICollection<Transcript> Transcripts { get; set; } = new List<Transcript>();
}