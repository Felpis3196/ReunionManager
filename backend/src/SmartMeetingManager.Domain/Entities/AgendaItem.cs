namespace SmartMeetingManager.Domain.Entities;

public class AgendaItem
{
    public Guid Id { get; set; }
    public Guid MeetingId { get; set; }
    public int Order { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TimeSpan? EstimatedDuration { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // Navigation properties
    public virtual Meeting Meeting { get; set; } = null!;
}